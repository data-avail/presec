Files in App dirctory (not subfolders!)
used for infastructure support, they cant be moved to other folders due to external frameworks restrictions
used frameworks:
1.Require.js - load scripts on fly (Scripts/require.js)
2.Handlebars - static templates (App/Handlebars)
3.Handlebars loader for require.js - static templates (https://github.com/SlexAxton/require-handlebars-plugin)
App/ hbs, json2, underscore, text
4.Knockout (files are in Scripts folder and loaded globally via html)
    a. knockout mapping plugin
    b. knockout validation plugin
