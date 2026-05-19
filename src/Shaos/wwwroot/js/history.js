"use strict";

const ctx = document.getElementById('chartCanvas').getContext('2d');
const chart = new Chart(ctx, {
    type: 'line',
    data: {
        datasets: [{
            fill: true,
            label: "",
            data: [],
            tension: 0.1
        }]
    }
});
