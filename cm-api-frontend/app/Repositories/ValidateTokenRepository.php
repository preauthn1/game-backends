<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\ValidateToken;
use DB;

/**
 * Class ValidateTokenRepository.
 */
class ValidateTokenRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return ValidateToken:[REDACTED];
    }

    public function getCountByMobile($member_id, $mobile)
    {
        return $this->model->where('mobile', $mobile)
            ->whereRaw('expire_date >= NOW()')
            ->where('member_id', '!=', $member_id)
            ->count();
    }

    public function checkTokenByMemberId($member_id, $type)
    {
        return $this->model->where('member_id', $member_id)
            ->where('status', 0)
            ->where('type_id', $type)
            ->where('expire_date', '>=', date('Y-m-d H:i:s'))
            ->first();
    }

    public function getValidateTokenByMobile($member_id, $mobile, $type)
    {
        return $this->model->where('member_id', $member_id)
            ->where('mobile', $mobile)
            ->where('status', 0)
            ->where('type_id', $type)
            ->where('expire_date', '>=', date('Y-m-d H:i:s'))
            ->first();
    }

    public function getValidateTokensCountByMobile($mobile, $organization_id, $member_id = 0)
    {
        return DB::table('validate_tokens as vt')
            ->join('members as m', 'm.id', 'vt.member_id')
            ->join('agents as a', function ($join) use ($organization_id) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.organization_id', $organization_id);
            })
            ->where('vt.mobile', $mobile)
            ->whereRaw('vt.expire_date >= NOW()')
            ->where('vt.member_id', '!=', $member_id)
            ->count();
    }

    public function createValidateTokenByMemberId($member_id, $type, $mobile, $token_expired, $token)
    {
        $data = array();
        $data['member_id'] = $member_id;
        $data['type_id'] = $type;
        $data['mobile'] = $mobile;
        $data['info'] = '';
        $data['token'] = $token;
        $data['expire_date'] = date('Y-m-d H:i:s', strtotime('+ ' . $token_expired . 'minute'));
        $data['status'] = 0;
        $data['times'] = 1;
        return $this->model->create($data);
    }

    public function updateTokenStatusByMemberId($member_id, $mobile, $type)
    {
        return $this->model->where('member_id', $member_id)
            ->where('mobile', $mobile)
            ->where('status', 0)
            ->where('type_id', $type)
            ->update(['status' => 2]);
    }

    public function incrementTimesAndExpired($model, $token_expired)
    {
        $model->increment('times');
        $model->expire_date = date('Y-m-d H:i:s', strtotime('+ ' . $token_expired . 'minute'));
        $model->save();
        return $model;
    }

    public function checkValidateToken($member_id, $type_id, $token)
    {
        return $this->model->where('member_id', $member_id)
            ->where('type_id', $type_id)
            ->where('token', $token)
            ->where('status', 0)
            ->first();
    }

    public function updateValidateTokenStatus($id)
    {
        return $this->model->where('id', $id)
            ->where('expire_date', '>=', date('Y-m-d H:i:s'))
            ->where('status', 0)
            ->update(['status' => 1]);
    }

    // 取TOKEN, 1:手機用的就為6碼數字，2:電子郵件用為32碼文字+數字
    public function getToken($verify_by)
    {
        if ($verify_by == 1) {
            $token = [REDACTED], 999999), 6, '0', STR_PAD_LEFT);
        } else {
            $token = [REDACTED];
        }

        $is_exist = $this->model->where('token', $token)
            ->where('expire_date', '>=', date('Y-m-d H:i:s'))
            ->count();
        // 驗證不重覆
        return ($is_exist) ? $this->getToken($this->model) : $token;
    }
}
