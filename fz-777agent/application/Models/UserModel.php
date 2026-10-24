<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class UserModel extends Model
{
    protected $table    = 'user';
    protected $fillable = ['username', 'weixin', 'phone', 'password', 'groupid', 'encrypt', 'inviteuid', 'wechat_openid', 'wechat_user', 'gid', 'note'];
    public $timestamps = false;

    /**
     * 获取登录类型
     * @param string $name 用户名
     * @return string
     */
    public static function getLoginType($name)
    {
        return 'username';
        $type = '';
        if (preg_match('/^1([0-9]{10})$/', $name)) {
            $type = 'phone';
        } elseif (preg_match('/^([a-z0-9_]+)@([a-z0-9_]+).([a-z]{2,6})$/', $name)) {
            $type = 'weixin';
        } else {
            $type = 'username';
        }
        return $type;
    }
	/**
	 * 密码加密
	 * @param string $password 原密码
	 * @param string $encrypt 加密串
	 * @return multitype:string
	 */
    public static function getPasswordEncrypt($password, $encrypt = '')
    {
        $return             = [];
        $return['encrypt']  = $encrypt ? $encrypt : self::createRandomstr();
        $return['password'] = md5(md5(trim($password)) . $return['encrypt']);
        return $encrypt ? $return['password'] : $return;
    }

    /**
     * 生成加密串
     * @param number $lenth 字符串长度
     * @return string
     */
    public static function createRandomstr($lenth = 6)
    {
        return self::random($lenth, '123456789abcdefghijklmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ');
    }

    public static function makeGid()
    {
        $gid1     = self::random(2, 'abcdefghijklmnpqrstuvwxyz');
        $gid2     = self::random(6);
        $gid      = $gid1 . $gid2;
        $userInfo = UserModel::where('gid', $gid)->first();
        if (isset($userInfo)) {
            self::makeGid();
        } else {
            return $gid;
        }
    }

    public static function random($length, $chars = '0123456789')
    {
        $hash = '';
        $max  = strlen($chars) - 1;
        for ($i = 0; $i < $length; $i++) {
            $hash .= $chars[mt_rand(0, $max)];
        }
        return $hash;
    }
	//检验用户名
    public static function checkUsername($username)
    {
        $strlen = strlen($username);
        if (self::isBadword($username) || !preg_match("/^[a-zA-Z0-9_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]+$/", $username)) {
            return false;
        } elseif (20 < $strlen || $strlen < 2) {
            return false;
        }
        return true;
    }
	//检验密码
    public static function checkPassword($password)
    {
        $strlen = strlen($password);
        if (self::isBadword($password)) {
            return false;
        }
        if ($strlen >= 6 && $strlen <= 20) {
            return true;
        }

        return false;
    }
	//特殊字符
    public static function isBadword($string)
    {
        $badwords = ["\\", '&', ' ', "'", '"', '/', '*', ',', '<', '>', "\r", "\t", "\n", "#"];
        foreach ($badwords as $value) {
            if (strpos($string, $value) !== false) {
                return true;
            }
        }
        return false;
    }
	//检查输入
    public static function checkInput($type, $string)
    {
        $return = false;
        if (!isset($string)) {
            return $return;
        }
        switch ($type) {
            case 'username':
                $return = self::checkUsername($string);
                break;
            case 'password':
                $return = self::checkPassword($string);
                break;
        }
        return $return;
    }
}
