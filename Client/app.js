const express = require('express');
const path = require('path');

const app = express();
const port = 3000;

const webDir = 'www';
const indexFile = 'index.html';

app.use(express.static(path.join(process.cwd(), webDir)));

app.get('/', (req, res) => {
    res.sendFile(path.join(process.cwd(), webDir, indexFile));
});

app.listen(port, () => {
    console.log(`listening on http://localhost:${port}`);
});
