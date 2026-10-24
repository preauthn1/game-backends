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
    public function sendByLocal($member_id, $mobile, $text, $sms_gateway)
    {
        //寫入簡訊紀錄
        $record_id = $this->addSmsSendRecord($member_id, $sms_gateway->country_id, $mobile, $text, $sms_gateway->sms_gateway_id);

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

    /* 使用簡訊商Huanxun58發送簡訊 */
    public function sendByHuanxun58($memberId, $mobile, $text, $sms_gateway)
    {
        try {
            $url = $sms_gateway->url . '/sms/Api/IntSend.do';

            $json = json_decode($sms_gateway->info);

            $data = array();
            $data['SpCode'] = isset($json->SpCode) ? urlencode($json->SpCode) : '';
            $data['LoginName'] = isset($json->LoginName) ? urlencode($json->LoginName) : '';
            $data['Password'] = isset($json->Password) ? urlencode($json->Password) : '';
            $data['MessageContent'] = isset($json->prefix) ? $json->prefix . ' ' . $text : $text;
            $data['UserNumber'] = $mobile;

            //寫入簡訊紀錄
            $record_id = $this->addSmsSendRecord($memberId, $sms_gateway->country_id, $mobile, $data['MessageContent'], $sms_gateway->sms_gateway_id);
            $sender = new VendorSend();
            $response = $sender->send('huanxun58', $url, 'GET', $data);
            $result = json_decode($response, true);

            // {"result":"0","description":"发送成功,需要人工审核", "taskid":"190319105738200000"}
            $transaction_id = '';
            $status = 2;
            $code = 9999;
            $message = 'unknow';
            if (is_array($result)) {
                if ($result['result'] == '0') {
                    //提交成功的json集合
                    $transaction_id = $result['taskid'];
                    $status = 1;
                    $code = 1000;
                    $message = __('error_message.1000');
                } else {
                    if ($result['result'] == '17') {
                        //该通道余额不足，不能满足本次发送
                        $sendMessage = '【huanxun58異常錯誤】 ' . PHP_EOL .
                            '名稱:' . $json->SpCode . PHP_EOL .
                            '狀態碼:' . $result['result'] . PHP_EOL .
                            '狀態說明:' . $result['description'];
                        $tgService = new TelegramService();
                        $tgService->sendMessage($sendMessage, 'warn');
                    }
                    $message = $result['result'] . ' ' . $result['description'];
                }
            }
            $this->updateSmsSendRecord($record_id, $status, json_encode($data), json_encode($result), $transaction_id);

            return [
                'code' => $code,
                'message' => $message
            ];
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            Common::log(
                $memberId,
                [
                    'code' => $info[0],
                    'msg' => $info[2] . ', ' . $exception->getMessage(),
                    'file' => $exception->getFile(),
                    'line' => $exception->getLine()
                ],
                'huanxun58 QueryException'
            );
            return [
                'code' => $info[0],
                'message' => $info[2]
            ];
        } catch (Exception $exception) {
            Common::log(
                $memberId,
                [
                    'code' => $exception->getCode(),
                    'msg' => $exception->getMessage(),
                    'file' => $exception->getFile(),
                    'line' => $exception->getLine()
                ],
                'huanxun58 Exception'
            );
            return [
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ];
        }
    }

    /* 使用簡訊商Quanqiu發送簡訊 */
    public function sendByQuanqiu($memberId, $mobile, $text, $sms_gateway)
    {
        try {
            $url = $sms_gateway->url . '/sms/mtsend';
            $json = json_decode($sms_gateway->info);

            $data = array();
            $data['appkey'] = isset($json->appKey) ? urlencode($json->appKey) : '';
            $data['secretkey'] = isset($json->secretKey) ? urlencode($json->secretKey) : '';
            $data['phone'] = $mobile;
            $data['content'] = isset($json->prefix) ? $json->prefix . ' ' . $text : $text;

            //寫入簡訊紀錄
            $record_id = $this->addSmsSendRecord(
                $memberId,
                $sms_gateway->country_id,
                $mobile,
                $data['content'],
                $sms_gateway->sms_gateway_id
            );
            $sender = new VendorSend();
            $response = $sender->send('Quanqiu', $url, 'POST', $data);
            $result = json_decode($response, true);

            // {"result":"0","description":"发送成功,需要人工审核", "taskid":"190319105738200000"}
            $transaction_id = '';
            $status = 2;
            $code = 9999;
            $message = 'unknow';

            $errorCode = array();
            $errorCode['0'] = '请求成功';
            $errorCode['1'] = '应用不可用或key错误';
            $errorCode['2'] = '参数错误或为空';
            $errorCode['3'] = '余额不足';
            $errorCode['4'] = '内容为空或包含非法关键词';
            $errorCode['5'] = '内容过长';
            $errorCode['6'] = '号码有误';
            $errorCode['7'] = '群发号码数量不得超过50000个';
            $errorCode['8'] = 'sourceaddress必须为3-10位数字或英文字母';
            $errorCode['9'] = 'IP非法';
            $errorCode['88'] = '请求失败';
            $errorCode['99'] = '系统错误';
            if (is_array($result)) {
                if ($result['code'] == '0') {
                    //提交成功的json集合
                    $transaction_id = $result['messageid'];
                    $status = 1;
                    $code = 1000;
                    $message = __('error_message.1000');
                } else {
                    if ($result['result'] == '3') {
                        //该通道余额不足，不能满足本次发送
                        $sendMessage = '【Quanqiu異常錯誤】 ' . PHP_EOL .
                            '名稱:' . $json->appKey . PHP_EOL .
                            '狀態碼:' . $result['code'] . PHP_EOL .
                            '狀態說明:' . $result['result'];
                        $tgService = new TelegramService();
                        $tgService->sendMessage($sendMessage, 'warn');
                    }
                    $message = $result['code'] . ' ' . $result['result'];
                }
            }
            $this->updateSmsSendRecord($record_id, $status, json_encode($data), json_encode($result), $transaction_id);

            return [
                'code' => $code,
                'message' => $message
            ];
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            Common::log(
                $memberId,
                [
                    'code' => $info[0],
                    'msg' => $info[2] . ', ' . $exception->getMessage(),
                    'file' => $exception->getFile(),
                    'line' => $exception->getLine()
                ],
                'Quanqiu QueryException'
            );
            return [
                'code' => $info[0],
                'message' => $info[2]
            ];
        } catch (Exception $exception) {
            Common::log(
                $memberId,
                [
                    'code' => $exception->getCode(),
                    'msg' => $exception->getMessage(),
                    'file' => $exception->getFile(),
                    'line' => $exception->getLine()
                ],
                'Quanqiu Exception'
            );
            return [
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ];
        }
    }

    /* 使用簡訊商Abosend發送簡訊 */
    public function sendByAbosend($memberId, $mobile, $text, $sms_gateway)
    {
        try {
            $url = $sms_gateway->url . '/v2/api/sendSMS';
            $json = json_decode($sms_gateway->info);

            $data = array();
            $data['orgCode'] = isset($json->orgCode) ? urlencode($json->orgCode) : '';
            $data['mobiles'] = $mobile;
            $data['content'] = isset($json->prefix) ? $json->prefix . ' ' . $text : $text;
            $data['rand'] = mt_rand(100000,999999);
            $MD5Key = isset($json->MD5Key) ? urlencode($json->MD5Key) : '';
            $data['sign'] = strtoupper(md5($data['orgCode'].$data['content'].$data['rand'].$MD5Key));

            //寫入簡訊紀錄
            $record_id = $this->addSmsSendRecord(
                $memberId,
                $sms_gateway->country_id,
                $mobile,
                $data['content'],
                $sms_gateway->sms_gateway_id
            );
            $headers = array('Content-Type: application/json');
            $sender = new VendorSend(['json_post' => true]);
            $response = $sender->send('Abosend', $url, 'POST', $data, $headers);
            $result = json_decode($response, true);

            // {"code":200,"data":{"sendCode":"20240219150724TYZIR"},"message":"Submitted successfully"}
            $transaction_id = '';
            $status = 2;
            $code = 9999;
            $message = 'unknow';

            if (is_array($result)) {
                if ($result['code'] == '200') {
                    //提交成功的json集合
                    $transaction_id = $result['data']['sendCode'];
                    $status = 1;
                    $code = 1000;
                    $message = __('error_message.1000');
                } else {
                    $message = $result['code'] . ' ' . $result['message'];
                }
            }
            $this->updateSmsSendRecord($record_id, $status, json_encode($data), json_encode($result), $transaction_id);

            return [
                'code' => $code,
                'message' => $message
            ];
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            Common::log(
                $memberId,
                [
                    'code' => $info[0],
                    'msg' => $info[2] . ', ' . $exception->getMessage(),
                    'file' => $exception->getFile(),
                    'line' => $exception->getLine()
                ],
                'Quanqiu QueryException'
            );
            return [
                'code' => $info[0],
                'message' => $info[2]
            ];
        } catch (Exception $exception) {
            Common::log(
                $memberId,
                [
                    'code' => $exception->getCode(),
                    'msg' => $exception->getMessage(),
                    'file' => $exception->getFile(),
                    'line' => $exception->getLine()
                ],
                'Quanqiu Exception'
            );
            return [
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ];
        }
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
