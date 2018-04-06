# WebMessenger



## Introduction

A Webmessenger using ASP.NET MVC Core 2,Entity Framework, Iota/Tangle protocol and Felandil's C# Library "Tangle.Net" to send messages via a distributed ledger.



## Features

### Mainfeatures  
- Messages are stored on a Distributed Ledger  
- User can send/receive messages  
- User Account system  
- Chat request system  

### Other  
- User can change the colorscheme ([img](https://puu.sh/zR8eH/60a1009d1e.png))  
- User can see the iota transaction in a tangle explorer


### Missing features

- Hash&Salt Passwords
- Allow users to connect to other iota node
- Resizing window doesnt break entire page


## Proof-Of-Work

Every step which needs Proof-Of-Work is opening a loading icon to signal something is happening ([pic](https://puu.sh/zR89r/d50efa9031.png)). This process is taking a long time (~10seconds), not because of inefficiency of the code, but just the way Iota works.



## The steps internally to start a chat with a user

1. User A makes a request
2. User A generates the receiving iota address
3. User B user accepts request
4. User B generates receiving iota address



## Screenshots

[General View](https://puu.sh/zR88X/fc75fb3f29.png)  
[Request System 1](https://puu.sh/zR8if/79072df0fb.png)  
[Request System 2](https://puu.sh/zR8kh/59b88e0486.png)  
[Chat](https://puu.sh/zR8sx/7586945c34.png)  
