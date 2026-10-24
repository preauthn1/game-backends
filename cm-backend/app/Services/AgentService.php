<?php

namespace App\Services;

use Carbon\Carbon;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Str;
use App\Helpers\Common;
use App\Repositories\AgentRepository;
use App\Repositories\AgentAccessTokenRepository;
use App\Repositories\AgentLoginLogRepository;
use App\Repositories\RoleRepository;
use Exception;

/**
 * Class AgentService.
 */
class AgentService extends BaseService
{
    public function __construct()
    {
        $this->repository = new AgentRepository();
    }

    public function validator($params, $rule)
    {
        //參數驗證
        $validator = Validator::make($params, [
            'username' => 'required|string|between:4,20',
            'password' => 'required|string|between:6,12',
        ], $this->validatorMessage);
        if ($validator->fails()) {
            return response()->json([
                'code' => 2002,
                'message' => $validator->errors()->all()
            ]);
        }
    }

    public function find($agent_id, $check_status = false)
    {
        $agent = $this->repository->find($agent_id);
        //檢查帳號是否存在
        if (!is_object($agent)) {
            throw new Exception(__('error_message.9998'), 9998);
        }
        //檢查帳號是否鎖定
        if ($check_status && $agent->status != 1) {
            throw new Exception(__('error_message.9006'), 9006);
        }
        return $agent;
    }

    public function findByUsername($username, $check_status = false)
    {
        $agent = $this->repository->findByUsername($username);
        //檢查帳號是否存在
        if (!is_object($agent)) {
            throw new Exception(__('error_message.1203'), 1203);
        }
        //檢查帳號是否鎖定
        if ($check_status && $agent->status != 1) {
            throw new Exception(__('error_message.9006'), 9006);
        }
        return $agent;
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->statusTxt = $item->status == 1 ? __('param.enable') : __('param.disable');
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            $item->last_login_time = Carbon::parse($item->last_login_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            return $item;
        });
        return [$total, $result];
    }

    public function getAgent($agent_id)
    {
        $agent = $this->repository->getAgent($agent_id);
        if (!$agent) {
            throw new Exception(__('error_message.1524'), 1524);
        }
        $agent->created_at = Carbon::parse($agent->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
        return $agent;
    }

    public function create($params)
    {
        //上級
        if (Common::IsNotNullOrEmptyString($params->parent_username)) {
            $agentNameData = $this->repository->findByUsername($params->parent_username);
            if (!$agentNameData) {
                throw new Exception(__('error_message.5041'), 5041);
            } else {
                $agent = $agentNameData;
            }
        }
        if (Common::IsNotNullOrEmptyString($params->referral_code)) {
            $agentCodeData = $this->repository->findByInviteCode($params->referral_code);
            if (!$agentCodeData) {
                throw new Exception(__('error_message.5010'), 5010);
            } else {
                $agent = $agentCodeData;
            }
        }
        if (empty($agent ?? [])) {
            //塞預設值
            $agent = $this->repository->getById(1);
        }
        //角色
        $roleRepository = new RoleRepository();
        $role = $roleRepository->getById($params->role_id);
        if (!$role) {
            throw new Exception(__('error_message.5042'), 5042);
        }

        $data = array();
        $data['username'] = $params->username;
        $data['nickname'] = $params->username;
        $data['password'] = Hash::make($params->password);
        $data['parent'] = $agent->id;
        $data['role_id'] = $params->role_id;
        $data['status'] = 1;
        $data['organization_id'] = $agent->organization_id;
        $data['customized_setting_group_id'] = $agent->customized_setting_group_id;
        $data['cash_flow_group_id'] = $agent->cash_flow_group_id;
        $data['sms_gateway_group_id'] = $agent->sms_gateway_group_id;
        $data['miner_group_id'] = $agent->miner_group_id;
        $data['created_by'] = 0;
        if (Common::IsNotNullOrEmptyString($params->sub_account) && $params->sub_account == 1) {
            $data['host_id'] = $agent->id;
            $data['code'] = '-';
        } else {
            $data['code'] = $this->repository->getInvitationCode();
        }
        $result = $this->repository->create($data);

        $updateData = array();
        $updateData['hierarchy_path'] = $agent->hierarchy_path . '/' . $result->id;
        $result = $this->repository->updateById($result->id, $updateData);
    }

    public function updateManager($params)
    {
        $manager = $this->find($params->manager_id);
        if (!is_object($manager)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->password)) {
            $data['password'] = Hash::make($params->password);
            $data['last_reset_password'] = Carbon::now();
        }
        if (Common::IsNotNullOrEmptyString($params->role_id)) {
            $data['role_id'] = $params->role_id;
        }
        if (Common::IsNotNullOrEmptyString($params->customized_setting_group_id)) {
            $data['customized_setting_group_id'] = $params->customized_setting_group_id;
        }
        if (Common::IsNotNullOrEmptyString($params->cash_flow_group_id)) {
            $data['cash_flow_group_id'] = $params->cash_flow_group_id;
        }
        if (Common::IsNotNullOrEmptyString($params->sms_gateway_group_id)) {
            $data['sms_gateway_group_id'] = $params->sms_gateway_group_id;
        }
        if (Common::IsNotNullOrEmptyString($params->miner_group_id)) {
            $data['miner_group_id'] = $params->miner_group_id;
        }
        if (isset($params->status)) {
            $data['status'] = $params->status;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateById($params->manager_id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }

    public function updateLastLogin($agent_id, $ip)
    {
        $result = $this->repository->updateLastLogin($agent_id, $ip);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function checkPassword($password, $agent_password)
    {
        //檢查帳號是否鎖定
        if (!Hash::check($password, $agent_password)) {
            throw new Exception(__('error_message.5003'), 5003);
        }
    }

    public function checkToken($token)
    {
        $agentAccessTokenRepository = new AgentAccessTokenRepository();
        $result = $agentAccessTokenRepository->findByToken($token);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9998'), 9998);
        }
        return $result;
    }


    public function getToken($agent_id)
    {
        $this->setTokenDisable($agent_id);
        $agentAccessTokenRepository = new AgentAccessTokenRepository();
        $data = array();
        $token = [REDACTED];
        $data['agent_id'] = $agent_id;
        $data['token'] = $token;
        $data['expire_date'] = date('Y-m-d H:i:s', strtotime('+ ' . env('TOKEN_EXPIRED', 1440) . 'minute'));
        $result = $agentAccessTokenRepository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $token;
    }

    public function setTokenDisable($agent_id)
    {
        $agentAccessTokenRepository = new AgentAccessTokenRepository();
        $agentAccessTokenRepository->setDisable($agent_id);
    }

    public function addLoginLog($agent_id, $ip, $ipv6 = null)
    {
        $agentLoginLogRepository = new AgentLoginLogRepository();

        $data = array();
        $data['agent_id'] = $agent_id;
        $data['ip'] = ip2long($ip);
        if (!empty($ipv6)) {
            $data['ipv6'] = $ipv6;
        }
        //$data['remark'] = '';
        $data['status'] = 1;
        $result = $agentLoginLogRepository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateData($agent_id, $data)
    {
        $result = $this->repository->updateByData($agent_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateGoogle2FaSecretKey($agent_id, $secret_key)
    {
        $data = array();
        $data['google2fa_secret'] = $secret_key;
        $data['is_2fa_enable'] = 0;
        return $this->updateData($agent_id, $data);
    }

    public function set2faEnable($agent_id, $is_2fa_enable)
    {
        if ($is_2fa_enable == 0) {
            $data = array();
            $data['is_2fa_enable'] = 1;
            return $this->updateData($agent_id, $data);
        }
        return true;
    }

    public function getCustomizedSetting(object $params)
    {
        if (empty($params->setting_key)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $this->repository = new AgentRepository();
        return $this->repository->getCustomizedSettings($params)->first();
    }

    public function getCustomizedSettings(object $params)
    {
        if (!isset($params->setting_keys) || !is_array($params->setting_keys)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $repository = new AgentRepository();
        return $repository->getCustomizedSettings($params)->get();
    }
}
