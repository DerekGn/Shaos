"use strict";

const historyData = JSON.parse(document.getElementById('history').innerHTML);
const settings = JSON.parse(document.getElementById('settings').innerHTML);
const container = document.getElementById('container');

var formattedData = formatHistoryValues(historyData.values);

const graph = new Dygraph(container, formattedData,
    {
        drawPoints: true,
        stepPlot: settings.stepPlot,
        fillGraph: true,
        valueRange: settings.range,
        labels: ['Time', 'Value']
    });

function formatHistoryValues(data) {
    let formattedData = [];

    for (let point of data) {
        formattedData.push([new Date(point.timeStamp), formatHistoryValue(point.value)])
    }

    return formattedData;
}

function formatHistoryValue(value) {
    if (typeof value == "boolean") {
        console.debug("is boolean");
        return value ? 1 : 0;
    } else {
        return value;
    }
}