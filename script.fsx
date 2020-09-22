#time "on"
#r "nuget: Akka.FSharp"                                                                 // importing Akka.Fsharp through NuGet Manager
#r "nuget: Akka.TestKit"                                                                // importing Akka.TestKit through NuGet Manager
#r "nuget: Akka.Configuration"                                                          // importing Akka.Configuration through NuGet Manager

open System                                                                             // Default Fsharp library for system functions
open Akka.Actor                                                                         // Akka.actor -> required for creating ActorySystem and Actors
open Akka.Configuration                                                                 // Required to set to default configuration
open Akka.FSharp                                                                        // Akka Fsharp support 
open Akka.TestKit

let rand = Random(1234)                                                                 // Taking a random generator with seed 1234

let args = Environment.GetCommandLineArgs()                                             // Getting the command line args

let system = System.create "system" (Configuration.defaultConfig())                     // Initalizing Actor system in the given context

let N = uint64 args.[3]                                                                 // Setting the parameter N from args

let K = uint64 args.[4]                                                                 // Setting the parameter K from args 

printfn "Passed arguments are [N: %u, K: %u]\n" N K                                     // Printing N and K

let NumThreads = 1000                                                                   // Number of Actors to be run parallelly

let Each = N / (NumThreads|>uint64)                                                     // Dividing the workload among each Actor

type SquaresCalculator = Calculator of uint64 * uint64 * uint64 * uint64                // Object with tuple of (uint64, uint64, uint64, uint64)

let SqrSum n:bigint =                                                                   // Function to calculate Sum of squares till given n
    (n * (n+1I) * (2I*n + 1I) + 1I)/6I

let isPerfectSquare (n: bigint) =                                                       // Function that tells given number n is a perfect square
    let h = n &&& (0xF |> bigint)                                                       // squares in Hexa Decimal end in 0,1,4,9 -> therefore taking the last digit
    if (h > 9I) then false
    else
        let list = [2I;3I;5I;6I;7I;8I]                                                  // if last digit is in this list then given n not a perfect square
        if List.exists ((<>) h) list then
            let root = ((n |> double |> sqrt) + 0.5) |> floor|> bigint                  // otherwise passing through sqrt function and finding the root
            root*root = n                                                               // if root is a perfect square then isPerfectSquare return true otherwise false
        else false

let processor (mailbox: Actor<_>) =                                                     // Creating child actor for checking special numbers in range [low..high] 
    let rec loop () = actor {
        let! Calculator(n, k, low, high) = mailbox.Receive ()
        for x in low..high do
            let Sum = SqrSum (bigint(x)+bigint(k)-1I) - SqrSum (bigint(x)-1I)           // Calculating x^2 + (x+1)^2 + ... + (x+k-1)^2                         
            if (isPerfectSquare Sum) then                                               // Checking if given number is a perfect square using isPerfectSquare function
                printfn "%i" x     
        return! loop ()
    }
    loop ()

let parent(parentmailbox: Actor<_>) =                                                   // Parent actor which can receive messages
    let childRefs = [for i in 1..NumThreads do yield (spawn system (string i) processor)] // Creating child actors to compute special numbers
    let rec parentLoop() = actor {                                                      // Taking the message which is of type Calculator
        let! msg = parentmailbox.Receive()                                              // Receiving messages from parentmailbox
        childRefs.Item (rand.Next() % NumThreads) <! msg                                // Assiging messages to random children
        return! parentLoop()                                                            
    }
    parentLoop()

let parentRef = spawn system "Parent" parent

let response = 
    if Each > 0UL then                                                                  // Packing ranges as messages and sending it to parent actor which in turn sends to child actors
        for i in 1..NumThreads do
            let low = ((i-1)|>uint64) * (Each |> uint64) + 1UL
            let high = ((i)|>uint64) * (Each |> uint64)
            parentRef <! Calculator(N, K, low, high)
    else                                                                                // If N is small
        parentRef <! Calculator(N, K, 1UL, N)

let out = parentRef.GracefulStop(TimeSpan.FromSeconds(6000.0))                           //GracefulStop stops the actors after all the actors finish executing or stops after 6000 Seconds

while not out.IsCompletedSuccessfully do                                                // Waits till all the actors completes their job 
    out.Status |> ignore

system.Terminate()                                                                      // Terminates the Actor System
