<?php

namespace App\Services;

use App\Enums\SmsGatewayEnum;
use App\SmsApi;
use App\Repositories\SmsGatewayRepository;
use App\Repositories\SmsTextRepository;
use App\Repositories\ValidateTokenRepository;
use Exception;
use DB;

/**
 * Class SmsService.
 */
class SmsService extends BaseService
{
    protected $smsTextRepository;
    protected $insert_characters = '';
    protected $left_credit_alert = 0;
    protected $msg_merchant_prefix = '';

    public function __construct()
    {
        $this->repository = new SmsGatewayRepository();
    }

    public function sendToken($agent_group_id, $member_id, $mobile, $token, $type, $country_id, $time = 1)
    {
        $gateway = $this->getSetting($agent_group_id, $mobile, $time);
        $gateway->info = json_decode($gateway->info, true);
        //return [$gateway, array(), array()];
        $text = $this->getText($agent_group_id, $gateway->info, $token, $type, $country_id);

        return [$gateway, $text];

        $sms = new SmsApi();
        switch ($gateway->code) {
            case SmsGatewayEnum::CODE_LOCAL: //local
                $result = $sms->sendByLocal($member_id, $country_id, $mobile, $text, $gateway, $type);
                break;
            case SmsGatewayEnum::CODE_HUANXUN58: //huanxun58
                $result = $sms->sendByHuanxun58($member_id, $country_id, $mobile, $text, $gateway, $type);
                break;
            case SmsGatewayEnum::CODE_QUANQIU: //quanqiu
                $result = $sms->sendByQuanqiu($member_id, $country_id, $mobile, $text, $gateway, $type);
                break;
            case SmsGatewayEnum::CODE_ABOSEND: //abosend
                $result = $sms->sendByAbosend($member_id, $country_id, $mobile, $text, $gateway, $type);
                break;
            default:
                $result = null;
                break;
        }

        if (!(isset($result) && $result['code'] == 1000)) {
            throw new Exception(__('error_message.2003'), 2003);
        }
        return true;
    }

    public function getTokenExpired($agent_group_id)
    {
        $gateway = DB::table('agent_groups as ag')
            ->join('sms_gateway_group_details as sd', 'sd.agent_group_id', 'ag.id')
            ->where('ag.id', $agent_group_id)
            ->where('ag.type', 3)
            ->select(
                'ag.id',
                'sd.setting'
            )
            ->first();
        $gatewayInfo = json_decode($gateway->setting);
        //Validate Token 有效時間; 預設6分鐘
        $token_expired = $token_expired = isset($gatewayInfo->token_expired)
            ? $gatewayInfo->token_expired
            : env('SMS_TOKEN_EXPIRED', 6);
        //簡訊有效時間最大發送次數; 預設3次
        $max_send = $max_send = isset($gatewayInfo->max_send)
            ? $gatewayInfo->max_send
            : env('SMS_MAX_SEND', 3);
        return [$token_expired, $max_send];
    }

    public function checkSmsCode($member_id, $type, $token)
    {
        $repository = new ValidateTokenRepository();
        $type_id = $type . '1';
        $validateToken = [REDACTED], $type_id, $token);
        if (!$validateToken) {
            throw new Exception(__('error_message.5002'), 5002);
        }
        return $validateToken;
    }


    public function updateValidateToken($validate_token_id)
    {
        $repository = new ValidateTokenRepository();
        $result = $repository->updateValidateTokenStatus($validate_token_id);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    private function getSetting($agent_group_id, $mobile, $time)
    {
        //取得簡訊商發送順序
        $gateways = $this->repository->getSendList($agent_group_id, $mobile);
        if (!$gateways) {
            throw new Exception(__('error_message.2003'), 2003);
        }

        $count = count($gateways);
        if ($count == 0) {
            // 查無該國家簡訊商戶
            throw new Exception(__('error_message.2003'), 2003);
        }

        if ($count == 1) {
            $time = 1;
        } else {
            //當前有效時間內發送次數 > 當前可用簡訊商戶數
            $time = ($time % $count);
        }
        $gateway = $gateways[($time - 1)];
        $gateway = $this->setDefaultParamToSmsInfo($gateway);
        return $gateway;
    }

    private function getText($agent_group_id, $info, $token, $type, $country_id)
    {
        // 取訊息
        $this->smsTextRepository = new SmsTextRepository();
        $smsText = $this->smsTextRepository->getText($agent_group_id, $type, $country_id);
        $text = (!is_object($smsText)) ? '{code}' : $smsText->text;

        // 調整訊息內容
        $token = [REDACTED], $info);
        $text = str_replace('{code}', $token, $text);
        return $text;
    }

    private function setDefaultParamToSmsInfo($gateway)
    {
        $info = json_decode($gateway->info, true);

        if (!isset($info['insert_characters'])) {
            $info['insert_characters'] = $this->insert_characters;
        }
        if (!isset($info['left_credit_alert'])) {
            $info['left_credit_alert'] = $this->left_credit_alert;
        }
        if (!isset($info['prefix'])) {
            $info['prefix'] = $this->msg_merchant_prefix;
        }

        $gateway->info = json_encode($info);
        return $gateway;
    }

    private function changeContent($token, $info)
    {
        if (isset($info['insert_characters']) && $info['insert_characters'] != '') {
            //$token = [REDACTED] ($s, $info) {return $s.$info['insert_characters'];}, str_split($token)));
            $ar = str_split($token);
            foreach ($ar as $key => &$value) {
                $value = $value . (($key != array_key_last($ar)) ? $info['insert_characters'] : '');
            }
            $token = [REDACTED];
        }
        return $token;
    }

}
