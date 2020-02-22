const express = require('express');
const path = require('path');

const app = express();
const port = process.env.PORT || 3000;

app.use(express.static(path.join(__dirname, '/www')))

app.get('/', function (req, res) {
    res.sendFile(__dirname + '/www/index.html');
});

app.listen(port, function () {
    console.log('listening on http://localhost:' + port);
});
