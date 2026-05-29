"use strict";

const json = document.getElementById('json').innerHTML;
var data = JSON.parse(json);

const ctx = document.getElementById('chartCanvas').getContext('2d');
const chart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: data.map(x => x.TimeStamp),
        datasets: [{
            data: data.map(x => x.Value),
        }]
    },
    options: {
        scales: {
            x: {
                type: 'time'
            }
        }
    }
});