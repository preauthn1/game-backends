<?php
namespace App\Module;

/**
 *  API接口公共模块
 */
class MysqlModule
{
    /**
     * 自定义sql方法
     * @param string $sql 自定义sql
     * @param array $cfg 数据库配置
     * @return boolean
     */
    public function querySql($sql,$cfg=[])
    {
        if(empty($cfg)){
            $cfg = $this->settings['db'];
        }
        $conn = mysqli_connect($cfg['host'],$cfg['username'],$cfg['password']);
        
        if (!$conn) {  
            die('Could not connect: ' . mysqli_error($conn));  
        }
        mysqli_query($conn , "set names utf8");
        mysqli_select_db($conn, $cfg['database']);
        $row = mysqli_query($conn, $sql);
        while($data=mysqli_fetch_array($row,MYSQLI_ASSOC))
        {
            $result[] = $data;
        }
        mysqli_close($conn);
        return $result;
    }
}
