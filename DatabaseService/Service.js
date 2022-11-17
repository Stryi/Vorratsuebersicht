//
// Node.js REST API Server für die Zusammenarbeit mit der App 'Vorratsübersicht'
//
// Copyright 2022 by Christian Stryi
//
// Modified: 16.11.2022
//

'use strict';

const express = require("express");
const sqlite  = require("sqlite3");
const fs      = require('fs');
const path    = require('path');

const app = express();

const DatabaseDirectory = __dirname + "\\db";

app.use(express.json());

app.get('/GetVersion', (request, response) => {

    LOG("GetVersion...");
    
    const Version = {
        Version: "0.10.0.0",
        Name: "0.10",
        Copyright: "2022 (c) by Christian Stryi",
        Description: "Vorratsübersicht Datenbankserver.",
        WelcomeMessage: "Alle Datenbankänderungen am Server werden um 00:00 Uhr wieder rückgängig gemacht."};
   
    response.json(Version);
})

app.get('/GetDatabases', (request, response) => {

    LOG("GetDatabases...");

    var databaseList = [];

    fs.readdir(DatabaseDirectory, (err, files) =>
    {
        files.forEach(file =>
        {
            var ext = path.extname(file);
            if (ext != ".db3")
                return;
    
            databaseList.push(file)
        });
        response.json(databaseList);
    });
})

app.post('/GetDatabaseFileInfo', (request, response) => {

    LOG("GetDatabaseFileInfo: " + request.body.Database);

    var databaseName = request.body.Database;

    var dbFileName = `./db/${databaseName}`;

    if (!fs.existsSync(dbFileName))
    {
        return response.status(202).json({status: "error", message: `Database file '${dbFileName}' does not exists.`});
    }

    var status = fs.statSync(dbFileName);

    return response.json({Length: status.size, FullName: dbFileName});
})

app.post('/ExecuteQuery', (request, response) => {

    LOG("ExecuteQuery: " + request?.body?.SqlCommand, JSON.stringify(request.body.Parameters));

    var sql          = request.body.SqlCommand;
    var parameters   = request.body.Parameters;
    var databaseName = request.body.Database;

    var dbFileName = `./db/${databaseName}`;

    var error = TestQueryParameters(sql, databaseName, dbFileName);
    if (error)
    {
        return response.status(202).json(error);
    }

    var db = new sqlite.Database(dbFileName);
    db.all(sql, parameters, function(err, rows)
    {
        db.close();
        if (err) return response.status(202).json({status: "error", message: err.message});
        response.json(rows);
    });
});

/// Liefert nach der Ausführung die Id des angelegten Datensatzes.
app.post('/ExecuteInsert', (request, response) => {

    LOG("ExecuteInsert: " + request?.body?.SqlCommand);
    
    var sql          = request.body.SqlCommand;
    var parameters   = request.body.Parameters;
    var databaseName = request.body.Database;

    var dbFileName = `./db/${databaseName}`;

    var error = TestQueryParameters(sql, databaseName, dbFileName);
    if (error)
    {
        return response.status(202).json(error);
    }

    var db = new sqlite.Database(dbFileName);
    db.run(sql, parameters, function(err)
    {
        if (err)
        {
            db.close();
            response.status(202).json({status: "error", message: err.message});
        }

        response.json(this.lastID);
    });
});

app.post('/ExecuteScalar', (request, response) => {

    LOG("ExecuteScalar: " + request?.body?.SqlCommand);
    
    var sql          = request.body.SqlCommand;
    var parameters   = request.body.Parameters;
    var databaseName = request.body.Database;

    var dbFileName = `./db/${databaseName}`;

    var error = TestQueryParameters(sql, databaseName, dbFileName);
    if (error)
    {
        return response.status(202).json(error);
    }

    var db = new sqlite.Database(dbFileName);
    db.get(sql, parameters, function(err, row)
    {
        db.close();
        if (err) return response.status(202).json({status: "error", message: err.message});
       
        var returnValue;

        const jsonText = JSON.stringify(
            row,
            (key, value) => ( returnValue = value),
          );
        response.json(returnValue);
    });
});

app.post('/ExecuteNonQuery', (request, response) => {

    LOG("ExecuteNonQuery: " + request?.body?.SqlCommand);
    
    var sql          = request.body.SqlCommand;
    var parameters   = request.body.Parameters;
    var databaseName = request.body.Database;

    var dbFileName = `./db/${databaseName}`;

    var error = TestQueryParameters(sql, databaseName, dbFileName);
    if (error)
    {
        return response.status(202).json(error);
    }

    var db = new sqlite.Database(dbFileName);
    db.run(sql, parameters, function(err)
    {
        db.close();
        if (err) 
            return response.status(202).json({status: "error", message: err.message});

        response.json(this.changes);
    });
});

app.listen(5000);


function LOG(text, additionalText = "")
{
    if (text.length > 150)
        text = text.substring(0, 150-3) + "...";

    if (additionalText != "")
        additionalText = " - " + additionalText;

    var d = new Date();
    console.log("[" + d.toLocaleString() + "] - " + text + additionalText);
} 

function TestQueryParameters(sql, databaseName, dbFileName)
{
    if (!sql)
    {
        return {status: "error", message: "No SqlCommand parameter in request body."};
    }

    if (!databaseName)
    {
        return {status: "error", message: "No Database parameter in request body."};
    }

    if (!fs.existsSync(dbFileName))
    {
        return {status: "error", message: `Database file '${dbFileName}' does not exists.`};
    }
} 

function sleep(ms) {
    return new Promise((resolve) => {
      setTimeout(resolve, ms);
    });
  }