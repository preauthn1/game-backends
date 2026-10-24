<?php

namespace App;

use Illuminate\Database\QueryException;
use App\Models\SmsSendRecord;
use App\Helpers\Common;
use App\Services\TelegramService;
use App\Helpers\VendorSend;
use DB;
use Log;
use Exception;

class SmsApi
{
    /* 不使用簡訊商發送簡訊, 直接回覆成功 */
    public function sendByLocal($member_id, $country_id, $mobile, $text, $sms_gateway, $type)
    {
        //寫入簡訊紀錄
        $record_id = $this->addSmsSendRecord($member_id, $country_id, $mobile, $text, $sms_gateway->sms_gateway_id, $type);

        $request = array();
        $response = array();
        $response['code'] = 1000;
        $response['message'] = 'success';
        $transaction_id = date('YmdHis');
        $status = 1;
        //更新簡訊紀錄
        $this->updateSmsSendRecord($record_id, $status, $request, $response, $transaction_id);
        $result = [
            'code' => 1000,
            'message' => __('error_message.1000')
        ];
        return $result;
    }

    private function convertToCountryId($country_id, $vendor_setting_country_id)
    {
        //target country_id 轉成 三方的country_id
        foreach ($vendor_setting_country_id as $each_id) {
            if ($each_id->COUNTRY_ELG == $country_id) {
                return $each_id->COUNTRY_Third;
            }
        }

        return "";
    }

    private function addSmsSendRecord($member_id, $country_id, $mobile, $text, $sms_gateway_id)
    {
        try {
            $record_data = array();
            $record_data['sms_gateway_id'] = $sms_gateway_id;
            $record_data['member_id'] = $member_id;
            $record_data['country_id'] = $country_id;
            $record_data['mobile'] = $mobile;
            $record_data['message_content'] = $text;

            $sms_send_record = SmsSendRecord::create($record_data);
            return $sms_send_record->id;
        } catch (QueryException $exception) {
            return false;
        } catch (Exception $exception) {
            return false;
        }
    }

    private function updateSmsSendRecord($record_id, $status, $request, $response, $transaction_id = '')
    {
        if ($record_id != false) {
            SmsSendRecord::onWriteConnection()->find($record_id)->update(
                [
                    'status' =>  $status,
                    'transaction_id' => $transaction_id,
                    'request_info' => json_encode($request),
                    'response_info' => json_encode($response)
                ]
            );
        }
    }
}