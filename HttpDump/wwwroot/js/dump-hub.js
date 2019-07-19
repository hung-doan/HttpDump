"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/dumphub").build();

$('#loading').text('starting...');
connection.start().then(function () {
    $('#loading').text('waiting for messages...');
}).catch(function (err) {
    $('#loading').text('error');
    return console.error(err.toString());
});


connection.on("ReceiveMessage", function (message) {
    var row = $("<tr/>");
    var dateCol = $("<td/>").text((new Date()).toISOString());
    var urlCol = $("<td/>").append("<span class='http-method'>" + message.method + "</span>")
                            .append("<span class='http-url'>" + message.url + "</span>");
    var bodyCol = $("<td/>").text(message.body);

    row.append(dateCol);
    row.append(urlCol);
    row.append(bodyCol);

    $('#http-log tbody').prepend(row);
});
