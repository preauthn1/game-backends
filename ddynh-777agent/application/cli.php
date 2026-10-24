<?php
#用例 /opt/php/bin/php /opt/nginx/html/web/application/cli.php CrontabController actionSchedule 'time=minute'

ini_set('display_errors', '1');
ini_set('date.timezone','Asia/Shanghai');
define('APP_PATH', __DIR__ . '/../application');
define('ANF_PATH', __DIR__ . '/../anframework');
define("APP_ROOT",dirname(dirname(__FILE__)));

require ANF_PATH . '/vendor/autoload.php';
require APP_PATH . '/constants/const.php';
// Instantiate the app
$settings = require APP_PATH . '/configs/base.php';
$app      = new \Slim\App($settings);

$container = $app->getContainer();
session_start();
$container['session'] = function ($c) {
    return new \SlimSession\Helper;
};

$capsule = new \Illuminate\Database\Capsule\Manager;
$capsule->addConnection($container['settings']['log'],'log');
$capsule->addConnection($container['settings']['center'],'center');
$capsule->addConnection($container['settings']['perGameLog'],'perGameLog');
$capsule->addConnection($container['settings']['db']);
$capsule->setAsGlobal();
$capsule->bootEloquent();
$container['db'] = $capsule;
$serverParams = $container['request']->getServerParams();
if (PHP_SAPI === 'cli')
{
    function phperrorhandler($errno, $errstr, $errfile, $errline) {
        $err = date('Y-m-d H:i:s') . ' --- ' ."cli error:{$errstr},errorno:{$errno} in {$errfile} on line:{$errline}".PHP_EOL;
        error_log($err, 3, APP_ROOT.'/logs/'.date('Y-m-d').'error.log');
    }

    // 设置用户定义的错误处理函数
    set_error_handler("phperrorhandler");
   $argv = $serverParams['argv'];
   $controller = isset($argv[1])?$argv[1]:[];
   if(empty($controller)){
      exit('不存在此控制器'."\n");
   }
   $controller = str_replace('/', '\\', $controller);
   $controller = 'App\Controllers\\'.$controller;
   $clsMethods = get_class_methods($controller);
   if(empty($clsMethods)){
      exit('不存在此控制器'."\n");
   }
   foreach ($clsMethods as $key => &$value) {
        $value = strtolower($value);
   }
   $actionFun = isset($argv[2])?$argv[2]:[];
   $params = [];
   if(isset($argv[3])){
      parse_str($argv[3], $params);
   }
   if(!in_array(strtolower($actionFun),$clsMethods)){
      exit('不存在此方法'."\n");
   }
   $controller = new $controller($container);
    try{
        $controller->$actionFun($params);
    }catch (Exception $exception){
        error_log(date('Y-m-d H:i:s') . ' --- ' . json_encode($_REQUEST). "\r\n" . $exception . "\r\n", 3, APP_ROOT.'/logs/'.date('Y-m-d').'error.log');
    }
}
