/// <reference path="typings/node/node.d.ts" />
/// <reference path="typings/express/express.d.ts" />
'use strict';
var express = require("express");
var multer = require('multer');
var app = express();
var port = process.argv[2] || 3000, url = process.argv[3] || '', basedir = process.argv[4] || '';
const hcc_1 = require("./hcc");
var storage = multer.diskStorage({
    destination: function (req, file, callback) {
        callback(null, 'c:/tmp');
    },
    filename: function (req, file, callback) {
        callback(null, file.originalname);
    }
});
var upload = multer({ storage: storage }).single('userPhoto');
app.get('/', function (req, res) {
    res.sendFile(__dirname + "/index.html");
});
app.get('/config', function (req, res) {
    hcc_1.hcc.getSite(res, basedir, req..substring(8) + "/.hccConfig.json");
});
app.post('/upload', function (req, res) {
    console.log("post");
    upload(req, res, function (err) {
        if (err) {
            return res.end("Error uploading file.");
        }
        res.end("File is uploaded");
    });
});
app.listen(3000, function () {
    console.log("Working on port 3000");
});
//# sourceMappingURL=app.js.map