<?php

namespace App\Services;

use App\Repositories\ValidateTokenRepository;
use Exception;

/**
 * Class ValidateTokenService.
 */
class ValidateTokenService extends BaseService
{
    public function __construct()
    {
        $this->repository = new ValidateTokenRepository();
    }

    public function checkMobileNumberExists($member_id, $mobile)
    {
        $count = $this->repository->getCountByMobile($member_id, $mobile);
        if ($count > 0) {
            throw new Exception(__('error_message.1102'), 1102);
        }
    }

    public function checkValidateTokenByMemberId($member_id, $type, $verify_by, $mobile, $max_send, $token_expired)
    {
        $validate_token = [REDACTED], $type, $verify_by);

        if (!is_object($validate_token) || $validate_token->mobile != $mobile) {
            //TODO 將舊TOKEN改為無效,

            //找不到有效時間內的token 或手機號碼不同就重新產生驗證碼
            $token = [REDACTED], $type, $verify_by, $mobile, $token_expired);
        } elseif ($validate_token->times < $max_send) {
            $this->incrementTimesAndExpired($validate_token, $token_expired);
            $token = [REDACTED];
            //$row->times = $validate_token->times;
        } else {
            // 系統錯誤，請稍後再試
            throw new Exception(__('error_message.2037'), 2037);
        }
        return $token;
    }

    public function createValidateTokenByMemberId($member_id, $type, $verify_by, $mobile, $token_expired)
    {
        $token = [REDACTED];
        $validateToken = [REDACTED]
            $member_id, $type . $verify_by, $mobile, $token_expired, $token
        );
        if (!is_object($validateToken)) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $token;
    }

    public function incrementTimesAndExpired($model, $token_expired)
    {
        $result = $this->repository->incrementTimesAndExpired($model, $token_expired);
        if (!is_object($result)) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    //取得認證碼
    public function getValidateToken($member_id, $type, $verify_by)
    {

    }

    //檢查簡訊
    public function checkValidateToken($member_id, $type, $token)
    {

    }


}
