"use strict";

const json = document.getElementById('history').innerHTML;
var historyData = JSON.parse(json);

const ctx = document.getElementById('chartCanvas').getContext('2d');
const chart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: historyData.Values.map(x => x.TimeStamp),
        datasets: [{
            data: historyData.Values.map(x => x.Value),
            label: historyData.Label
        }]
    },
    options: {
        scales: {
            x: {
                type: 'time',
                time: {
                    displayFormats: {
                        day: 'MMM DD, YYYY'
                    }
                }
            }
        }
    }
});