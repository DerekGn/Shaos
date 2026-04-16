"use strict";

const eventSource = new EventSource('/api/v1/events');
const elementCount = 3;

window.onload = function () {

    eventSource.addEventListener('parameter-updated-event-bool', (event) => {
        console.debug('parameter-updated-event-bool');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[1].children[0].children[0].checked = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();
        }
    });

    eventSource.addEventListener('parameter-updated-event-float', (event) => {
        console.debug('parameter-updated-event-float');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {

            }
        }
    });

    eventSource.addEventListener('parameter-updated-event-int', (event) => {
        console.debug('parameter-updated-event-int');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {

            }
        }
    });

    eventSource.addEventListener('parameter-updated-event-string', (event) => {
        console.debug('parameter-updated-event-string');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {

            }
        }
    });

    eventSource.addEventListener('parameter-updated-event-uint', (event) => {
        console.debug('parameter-updated-event-uint');

        const parameter = JSON.parse(event.data);
        const element = document.getElementById(parameter.parameterId);

        if (element != null && element.children.length == elementCount) {
            element.children[0].innerHTML = parameter.value;
            element.children[2].innerHTML = new Date(parameter.timestamp).toLocaleString();

            if (parameter.canWrite) {

            }
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
