<?php

namespace App\Services;

use App\Repositories\CountryRepository;
use App\Helpers\Common;
use Exception;

/**
 * Class CountryService.
 */
class CountryService extends BaseService
{
    public function __construct()
    {
        $this->repository = new CountryRepository();
    }

    public function getDropdown()
    {
        return $this->repository->getDropdown();
    }

    public function getCountryIdByMobile($mobile)
    {
        // 移除電話號碼中的非數字字符
        $mobile = preg_replace('/\D/', '', $mobile);

        // 如果電話號碼開頭為 "00"，替換為 "+"
        if (strpos($mobile, '00') === 0) {
            $mobile = '+' . substr($mobile, 2);
        }

        // 查詢所有國家的國碼
        $countries = $this->repository->findAll();

        // 檢查電話號碼是否以某個國碼開頭
        foreach ($countries as $country) {
            if (strpos($mobile, $country['country_code']) === 0) {
                return $country->id;
            }
        }

        // 如果找不到匹配的國家
        return 0;
    }

    public function find($id)
    {
        $data = $this->repository->getById($id);
        if (!is_object($data)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $data;
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
        return [$total, $result];
    }

    public function create($params)
    {
        $data = array();
        $data['country_code'] = $params->countryCode;
        $data['code'] = $params->isoCode;
        $data['name'] = $params->countryName;
        $data['phone_rule'] = $params->phoneRule;
        $data['status'] = $params->status;
        if (Common::IsNotNullOrEmptyString($params->order)) {
            $data['sort'] = $params->order;
        }
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $originData = $this->find($params->id);
        if (!is_object($originData)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->countryCode)) {
            $data['country_code'] = $params->countryCode;
        }
        if (Common::IsNotNullOrEmptyString($params->isoCode)) {
            $data['code'] = $params->isoCode;
        }
        if (Common::IsNotNullOrEmptyString($params->countryName)) {
            $data['name'] = $params->countryName;
        }
        if (Common::IsNotNullOrEmptyString($params->phoneRule)) {
            $data['phone_rule'] = $params->phoneRule;
        }
        if (isset($params->order)) {
            $data['sort'] = $params->order;
        }
        if (isset($params->status)) {
            $data['status'] = $params->status;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateById($params->id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }
}
