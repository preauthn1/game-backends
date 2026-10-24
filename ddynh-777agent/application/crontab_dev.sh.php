#!/bin/bash
#<?php die(); ?>
#测试服脚本！！！
source /etc/profile
umask 002
#切换到当前目录
path=$0
path=`/usr/bin/dirname $path`
#cd $(cd "$(dirname "$0")";pwd)
readonly path=${path}/;
readonly minute=$((10#$(date +%M)))
readonly hour=$(date +%k)
readonly day=$((10#$(date +%d)))
readonly month=$(date +%m)
readonly weekday=$(date +%w)
readonly Ymd=$(date +%Y%m%d)

#每1分钟执行的任务

#每5分钟执行的任务
if [ "3" -eq "$(($minute % 5))" ]; then
    /usr/local/php/bin/php -f ${path}cli.php CronCliController insertByPerGameLog > /dev/null 2>&1 &   #牌局
fi

#每2分钟执行的任务
if [ "0" -eq "$(($minute % 2))" ]; then
    /usr/local/php/bin/php -f ${path}cli.php AutoController actionPayExchange > /dev/null 2>&1 &   #自动打款
fi

#每2分钟执行的任务
if [ "1" -eq "$(($minute % 2))" ]; then
    /usr/local/php/bin/php -f ${path}cli.php AutoController actionPayExchangeQuery > /dev/null 2>&1 &   #自动打款查询
fi

#每3分钟执行的任务
if [ "0" -eq "$(($minute % 3))" ]; then
    /usr/local/php/bin/php -f ${path}cli.php CronCliController handleNotDeLiver > /dev/null 2>&1 &   #发货
fi

#每30分钟执行的任务，每小时第2和32分钟执行
if [ "2" -eq "$(($minute % 30))" ]; then
    /usr/local/php/bin/php -f ${path}cli.php StatCliController online > /dev/null 2>&1 &   #在线统计
fi

#每1小时执行的任务，每小时第1分钟执行
if [ "1" -eq "$minute" ] ; then
    /usr/local/php/bin/php -f ${path}cli.php StatCliController dayDetailReport > /dev/null 2>&1 &   #系统日报
    /usr/local/php/bin/php -f ${path}cli.php StatCliController channelDetailReport > /dev/null 2>&1 &  #系统渠道日报
fi
