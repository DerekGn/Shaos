"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/runtimeHub").build();


//Disable the start button until connection is established.
document.getElementById("startButton").disabled = true;

