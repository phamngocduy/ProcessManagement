  var createError = require('http-errors');
  var express = require('express');
  var path = require('path');
  var favicon = require('serve-favicon');
  var logger = require('morgan');
  var mongoose = require('mongoose');
  var flash    = require('connect-flash');
  var passport = require('passport');
  var session      = require('express-session');
  var cookieParser = require('cookie-parser');
  var bodyParser   = require('body-parser');
  var async = require("async");
  var pathToRegexp = require('path-to-regexp')
  var app = express();
  var LocalStrategy   = require('passport-local').Strategy;
  var hosting = true,virtual_url;
  //set path
  var mypath;
  if(hosting){
    mypath='/CMU/K21T11/SEP21_EveProject';
    virtual_url = mypath; 
  }else{
    mypath='';
    virtual_url = '/';
  }
  global.virtual_path = mypath;


  //database
  // var configdb = require('./config/database.js');
  var configdb = require('config');
  mongoose.connect(configdb.url);
  // mongoose.connect(configdb.DBHost);
  mongoose.Promise =global.Promise;
  //pastpost
  require('./config/passport')(passport); 

  // view engine setup
  app.set('views', path.join(__dirname, 'app/views'));
  app.set('view engine', 'ejs');


  app.use(logger('dev',{'skip':skipLog}));
  // app.use(express.json());
  app.use(bodyParser.urlencoded({ extended: true }));
  app.use(bodyParser.json());
  app.use(cookieParser());
  app.use(express.static(path.join(__dirname,'public')));
  // app.use(express.static('public'));
  // console.log('mypath:'+mypath)
  app.use(session({ secret: 'faklksddsadjahsjm',
          cookie: { maxAge: 2628000000 },
          resave:false,
          saveUninitialized:false
  }));

  app.use(passport.initialize());
  app.use(passport.session()); 
  app.use(flash());


  //models
  var Course = require('./app/models/course');
  var Attendance = require('./app/models/attendance');
  var Session = require('./app/models/session');
  var Student = require('./app/models/student');


  //router
  var routes = require('./app/routes');
  // var users = require('./routes/users');
  // app.use('/', indexRouter);
  // app.use('/users', usersRouter);
  
  // app.use('/',routes);
  app.use(virtual_url,routes);
  // if(hosting){
  //   app.use('/CMU/K21T11/SEP21_EveProject',routes);
  // }else{
  //   app.use('/',routes);
  // }
  
  // catch 404 and forward to error handler
  app.use(function(req, res, next) {
    next(createError(404));
  });

  // error handler
  app.use(function(err, req, res, next) {
    // set locals, only providing error in development
    res.locals.message = err.message;
    res.locals.error = req.app.get('env') === 'development' ? err : {};

    // render the error page
    res.status(err.status || 500);
    if(res.status(404)){
      res.render('404');
    }else{
      res.render('error');
    }
  });
  function skipLog (req, res) {
    var url = req.url;
    if(url.indexOf('?')>0)
      url = url.substr(0,url.indexOf('?'));
    if(url.match(/(map|css|js|jpg|png|ico|gif|woff|woff2|eot)$/ig)) {
      return true;
    }
    return false;
  }

  module.exports = app;
