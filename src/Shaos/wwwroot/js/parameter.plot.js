"use strict";

const settings = JSON.parse(document.getElementById('settings').innerHTML);
const ctx = document.getElementById('chartCanvas').getContext('2d');
const chart = new Chart(ctx, {
    type: 'line',
    data: {
        datasets: [
            {
                label: settings.label,
                data: []
            }
        ]
    },
    options: {
        scales: {
            xAxes: [{
                type: 'realtime',
                delay: 0,
                // 20 seconds of data
                duration: 20000
            }],
            yAxes: [{
                ticks: {
                    suggestedMin: 0,
                    suggestedMax: 50
                }
            }]
        }
    }
});

var startButton = document.getElementById("start");
var stopButton = document.getElementById("stop");
var connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/plotHub")
    .build();

window.onload = function () {

    stopButton.disabled = true;

    connection.start().then(function () {
        console.log("Connected");
    }).catch(function (err) {
        return console.error(err.toString());
    });

    startButton.addEventListener("click", function () {
        connection.invoke("start", settings.id).catch(function (err) {
            return console.error(err.toString());
        });

        startButton.disabled = true;
        stopButton.disabled = false;
    });

    stopButton.addEventListener("click", function () {
        connection.invoke("stop", settings.id).catch(function (err) {
            return console.error(err.toString());
        });

        startButton.disabled = false;
        stopButton.disabled = true;
    });
}

connection.on("update", function (value, timestamp) {

    chart.data.labels.push(timestamp);
    chart.data.datasets.forEach((dataset) => {
        dataset.data.push(value);
    });

    chart.update();

    if (chart.data.labels.length > data.limit) {
        chart.data.labels.splice(0, 1);
        chart.data.datasets.forEach((dataset) => {
            dataset.data.splice(0, 1);
        });
        chart.update();
    }
});