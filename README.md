# WebMessenger

## Introduction

A Webmessenger using ASP.NET MVC Core 2,Entity Framework, Iota/Tangle protocol and Felandil's C# Library "Tangle.Net" to send messages via a distributed ledger.

## Features

..* User can change the colorscheme ([img](https://puu.sh/zR8eH/60a1009d1e.png))

## The steps internally to start a chat with a user are:

1. User A makes a request
2. User A generates the receiving iota address
3. User B user accepts request
4. User B generates receiving iota address

Every step which needs Proof-Of-Work is opening a loading icon to signal something is happening ([pic](https://puu.sh/zR89r/d50efa9031.png)).


![screenshot](https://puu.sh/zR88X/fc75fb3f29.png)
