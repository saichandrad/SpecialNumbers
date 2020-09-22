COP5615 - Distributed Operating System Principles
Project – 1

Members:
-->Sai Chandra Sekhar Devarakonda (UFID: 9092-2981)
-->Sumanth Chowdary Lavu (UFID: 5529-6647)

Objective:
-->The goal of this project is to use F# and actor model to build a good solution to find the sequence of squares of integers that leads to a perfect square and runs effectively on multi-core machines.

Algorithm:
-->In this program, after getting the input from the user in the form of (N,k), where k is the number of integers in the sequence and N is the final number for the sequence, We designate each actor with a range of sequences (work unit) for which it has to find whether the sum of squares of those integers is a square of an integer or not. If the result is true, then the starting number of that sequence will be printed. Once the actor computes all sequences assigned to it, then it reports the process spawned it about it’s exit.

Execution:
-->dotnet fsi --langversion:preview script.fsx N k
For example: dotnet fsi --langversion:preview script.fsx 1000000 4

Size of the work unit and running time:
-->For the problem of N = 1000000 and k= 4, the work unit was 1000 where the real time was 00:00:01.100 and CPU time was 00:00:03.504 and CPU utilization was 3.19.

Result of N = 1000000, k = 4:
-->Returns no output as there are no such numbers.

Largest problem successfully solved:
-->N = 1000000000, k = 24



