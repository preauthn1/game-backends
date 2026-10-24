<?php
/**
 * 存储过程调用类
 */

namespace App\Libraries;

class Procedure
{
    private static $_instance;
    private $conn;

    private function __construct($dbtype)
    {
        $baseConfig = include(APP_PATH . '/configs/base.php');
        $dbconfig = $baseConfig['settings'][$dbtype];
        $host = $dbconfig['host'];
        $database = $dbconfig['database'];
        $username = $dbconfig['username'];
        $password = [REDACTED]'password'];
        $connectionInfo = array("Database" => $database, "UID" => $username, "PWD" => $password, "CharacterSet" => "UTF-8");
        $this->conn = sqlsrv_connect($host, $connectionInfo);
    }

    public static function getInstance($dbtype)
    {
        ini_set('mssql.charset', 'UTF-8');
        if (!(self::$_instance[$dbtype] instanceof self)) {
            self::$_instance[$dbtype] = new self($dbtype);
        }
        return self::$_instance[$dbtype];
    }

    /**
     * 调用存储过程
     * @param string $procedureName 存储过程名
     * @param array $params 参数
     * @return bool
     */
    public function query($procedureName, $params)
    {
        mssql_query("SET ANSI_NULLS, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL, ANSI_WARNINGS, ANSI_PADDING ON");
        $stmt = mssql_init($procedureName, $this->conn);
        if (!$stmt) {
            return false;
        }
        if (!$params) return false;
        foreach ($params as $k => $v) {
            $output = isset($v['output']) && $v['output'] == true ? true : false;
            mssql_bind($stmt, "@{$k}", $v['value'], $v['type'] == 'int' ? SQLINT4 : SQLVARCHAR, $output);
        }
        mssql_bind($stmt, "RETVAL", $returnValue, SQLINT4, true);
        mssql_execute($stmt, false);
        return $returnValue;
    }


    public function updateGold($arr, $type)
    {
        if (!$this->conn) return false;

        $sp = $type == '1' ? "{?=call PrPsWeb_UpdateUserBank(?,?,?,?,?,?,?)}" : "{?=call PrWeb_Pay_FinishedAgent(?,?,?,?,?,?,?)}";
        error_log(date('Y-m-d H:i:s') . ' --- ' . implode($arr) . $sp. "\r\n", 3, APP_ROOT . '/logs/' .'test.txt');
        $ReturnValue = '';
        $tid1 = $arr[0];
        $tid2 = $arr[1];
        $tid3 = $arr[2];
        $tid4 = $arr[3];
        $tid5 = $arr[4];
        $tid6 = 0;
        $tid7 = '';

        $params = array(
            array(&$ReturnValue, SQLSRV_PARAM_OUT),
            array($tid1, SQLSRV_PARAM_IN),
            array($tid2, SQLSRV_PARAM_IN),
            array($tid3, SQLSRV_PARAM_IN),
            array($tid4, SQLSRV_PARAM_IN),
            array($tid5, SQLSRV_PARAM_IN),
            array(&$tid6, SQLSRV_PARAM_INOUT),
            array(&$tid7, SQLSRV_PARAM_INOUT)
        );
        sqlsrv_query($this->conn, $sp, $params);
        return $ReturnValue;
    }


    public function sendSysMail($arr)
    {
        if (!$this->conn) return false;

        $sp = "{?=call PrPs_SendSysMail(?,?,?,?,?)}";
        $ReturnValue = '';
        $tid1 = $arr[0];
        $tid2 = $arr[1];
        $tid3 = $arr[2];
        $tid4 = $arr[3];
        $tid5 = '';

        $params = array(
            array(&$ReturnValue, SQLSRV_PARAM_OUT),
            array($tid1, SQLSRV_PARAM_IN),
            array($tid2, SQLSRV_PARAM_IN),
            array($tid3, SQLSRV_PARAM_IN),
            array($tid4, SQLSRV_PARAM_IN),
            array(&$tid5, SQLSRV_PARAM_INOUT)
        );

        sqlsrv_query($this->conn, $sp, $params);
        return $ReturnValue;
    }

    public function select($sql)
    {
        if (!$this->conn) return false;
        mssql_query("SET ANSI_NULLS, QUOTED_IDENTIFIER, CONCAT_NULL_YIELDS_NULL, ANSI_WARNINGS, ANSI_PADDING ON");
        $res = mssql_query($sql);
        return mssql_fetch_assoc($res);
    }
}