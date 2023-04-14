debugger;

const express = require("express");
const app = express();

app.get('/test', (req,resp) => {
    const products = [
        {id: 1, name: "hammer"},
        {id: 2, name: "screwdriver"},
        {id: 3, name: "wrench"}
      ];
   
      resp.json(products);})

app.listen(3000);
