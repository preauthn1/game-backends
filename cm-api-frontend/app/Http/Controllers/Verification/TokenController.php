<?php

namespace App\Http\Controllers\Verification;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\AgentDomainService;
use App\Services\MailService;
use App\Services\MemberService;
use App\Services\AgentService;
use App\Services\ValidateTokenService;
use App\Services\SmsService;
use App\Models\SimpleNamedLock;
use App\Enums\ValidateTokenEnum;
use Validator;
use Exception;

class TokenController extends Controller
{
    /**
     * 發送驗證碼
     *
     * @response {
     *     "code": 1000,
     *     "message": "Success",
     *     "data": {
     *         "key": "$2y$12$0/LCUWT3QHPIjeVyjzgNSuZN4QHcz5k8K2fu7VxLND.SPH1k6ag8C",
     *         "image_url": "data:image/png;base64,iVBORw0KGgo....RK5CYII=",
     *     }
     * }
     *
     */
    public function sendValidateCode(Request $request)
    {
        try {
            $rules = [
                'domain' => 'required|domain',
                'type' => 'in:1,2,3,4,5,6,7,8',
                'member_id' => 'digits_between:1,10',
                'username' => 'r_alpha_num|between:4,30',
                'verify_by' => 'in:1,2',
                'email' => ($request->type == 1) ? 'email|between:0,60|unique:members,email' : 'email|between:0,60',
                'country_id' => ($request->verify_by == 1) ? 'required|digits_between:0,3' : 'digits_between:0,3',
                'mobile' => ($request->verify_by == 1) ? 'required|digits_between:0,20' : 'digits_between:0,20',
                'payment_password' => ($request->type == 5) ? 'required|between:6,12|confirmed' : 'between:6,12|between:6,12'
            ];

            // 驗證POST資料
            $validator = Validator::make($request->all(), $rules, $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $verify_mobile = $request->verify_by == ValidateTokenEnum::VERIFY_MOBILE;
            $verify_email = $request->verify_by == ValidateTokenEnum::VERIFY_EMAIL;

            $agentDomainService = new AgentDomainService();
            $agent_id = $agentDomainService->getAgentIdByDomain($request->domain);
            $agentService = new AgentService();
            $agent = $agentService->find($agent_id);
            $organization_id = is_object($agent) ? $agent->organization_id : 0;
            if ($organization_id == 0) {
                throw new Exception(__('error_message.9997'), 9997);
            }

            //檢查前先Lock
            $lock = new SimpleNamedLock('ValidateTokenController', 0, $request->mobile);

            $service = new MemberService();
            //檢查手機號是否重複
            $service->checkMobileNumberExists($request->mobile, $organization_id);

            //找帳號是否存在
            if (in_array(
                $request->type,
                [
                    ValidateTokenEnum::TYPE_FORGET_PASSWORD, // 2.忘記密碼
                    ValidateTokenEnum::TYPE_FORGET_PAYMENT_PASSWORD // 2.忘記資金密碼
                ]
            )) {
                $member = $service->findByMobile($request->mobile);
            } elseif ($request->type == ValidateTokenEnum::TYPE_ONE_PAGE_REGISTRATION) {
                $agentDomainService = new AgentDomainService();
                $agent_id = $agentDomainService->getAgentIdByDomain($request->domain);
                $member = $service->getSystemDetect('Registrants', $agent_id);

                $agentService = new AgentService();
                $agent = $agentService->find($member->agent_id);
                $request->SMS_GATEWAY_GROUP = $agent->sms_gateway_group_id;
            } else {
                $member = $service->find($request->ID);
            }

            $validateTokenService = new ValidateTokenService();
            if ($request->type != 8) {
                $validateTokenService->checkMobileNumberExists($request->ID, $request->mobile);
            }

            //取得token expired時間
            $smsService = new SmsService();
            $agent_group_id = $request->SMS_GATEWAY_GROUP ?? 3;
            list($token_expired, $max_send) = $smsService->getTokenExpired($agent_group_id);
            $token = [REDACTED]
                $member->id, $request->type, $request->verify_by, $request->mobile, $max_send, $token_expired
            );
            if ($request->verify_by == ValidateTokenEnum::VERIFY_EMAIL) {
                $mailService = new MailService();
                $result = $mailService->sendToken($member, $request, $token);
                // mail 驗證
                if (count($this->forgetPasswordMail($request->email, $token)) > 0) {
                    $return_value = false;
                }
            } else {
                //TODO
                $smsService = new SmsService();
                $result = $smsService->sendToken(
                    $request->SMS_GATEWAY_GROUP,
                    $member->id,
                    $request->mobile,
                    $token,
                    $request->type,
                    $request->country_id,
                    1//$time
                );
            }

            $lock->unLock();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
            ]);

        } catch (QueryException $exception) {
            $lock->unLock();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            $lock->unLock();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage(),
                'file' => $exception->getFile(),
                'line' => $exception->getLine()
            ]);
        }
    }

}
