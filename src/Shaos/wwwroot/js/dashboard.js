"use strict";

const eventSource = new EventSource('/api/v1/events');

window.onload = function () {

    eventSource.addEventListener('parameter-updated-event-bool', (event) => {
        console.debug('parameter-updated-event-bool');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null) {
        }
    });

    eventSource.addEventListener('parameter-updated-event-float', (event) => {
        console.debug('parameter-updated-event-float');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null) {
        }
    });

    eventSource.addEventListener('parameter-updated-event-int', (event) => {
        console.debug('parameter-updated-event-int');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null) {
        }
    });

    eventSource.addEventListener('parameter-updated-event-string', (event) => {
        console.debug('parameter-updated-event-string');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null) {
        }
    });

    eventSource.addEventListener('parameter-updated-event-uint', (event) => {
        console.debug('parameter-updated-event-uint');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null) {
        }
    });

    eventSource.onerror = (err) => {
        console.error('EventSource failed:', err);
        eventSource.close();
    };

    eventSource.onopen = () => {
        console.log('Connection to server opened.');
    };
}
