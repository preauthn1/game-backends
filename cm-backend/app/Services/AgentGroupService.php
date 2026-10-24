<?php

namespace App\Services;

use App\Enums\PublicEnum;
use App\Helpers\Common;
use App\Repositories\AgentGroupRepository;
use App\Repositories\AgentSettingRepository;
use Exception;

class AgentGroupService extends BaseService
{
    public function __construct()
    {
        $this->repository = new AgentGroupRepository();
    }

    public function getTypeDropdown($params)
    {
        $result = $this->repository->getTypeDropdown($params);
        return $result->map(function ($item) {
            $item->id = $item->type_id;
            $item->name = __('param.agent_group.type.' . $item->type_id);
            return $item;
        });
    }

    public function getDropdown($params)
    {
        return $this->repository->getDropdown($params);
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $data = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = [];
        $data->map(function ($item) use (&$result) {
            if (empty($result[$item->parent_id])) {
                $item->mainName = $this->getLanguageTxt($item->parent_txt);
                $item->subName = $this->getLanguageTxt($item->language_text);
                $result[$item->parent_id] = [
                    'mainId' => $item->parent_id,
                    'mainName' => $item->mainName,
                    'sub' => []
                ];
                $result[$item->parent_id]['sub'][] = [
                    'id' => $item->id,
                    'subId' => $item->define_id,
                    'subName' => $item->subName,
                    'dataType' => $item->data_type,
                    'dataTypeTxt' => __('param.data_type.' . $item->data_type),
                    'value' => $item->value,
                ];
            } else {
                $item->subName = $this->getLanguageTxt($item->language_text);
                $result[$item->parent_id]['sub'][] = [
                    'id' => $item->id,
                    'subId' => $item->define_id,
                    'subName' => $item->subName,
                    'dataType' => $item->data_type,
                    'dataTypeTxt' => __('param.data_type.' . $item->data_type),
                    'value' => $item->value,
                ];
            }
        });
        $result = collect($result)->values()->all();
        return [$total, $result];
    }

    public function view($params)
    {
        $result = $this->repository->view($params);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $result->dataTypeTxt = __('param.data_type.' . $result->data_type);
        $result->mainName = $this->getLanguageTxt($result->parent_txt);
        $result->subName = $this->getLanguageTxt($result->language_text);
        return $result;
    }

    public function create($params)
    {
        $data = array();
        $data['type'] = $params->group_type;
        $data['name'] = $params->name;
        if (Common::IsNotNullOrEmptyString($params->memo)) {
            $data['memo'] = $params->memo;
        }
        $data['organization_id'] = $params->ORGANIZATION_ID;
        $data['is_default'] = 0;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $agentSettingRepository = new AgentSettingRepository();
        $setting = $agentSettingRepository->getById($params->setting_id);
        if (!$setting) {
            throw new Exception(__('error_message.9001'), 9001);
        }

        $data = array();
        if (isset($params->value)) {
            $data['value'] = $params->value;
        }
        if (count($data) > 0) {
            $result = $agentSettingRepository->updateById($params->setting_id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }

    private function getLanguageTxt($list = '')
    {
        $result = '';
        if (empty($list)) {
            return $result;
        }
        $nowLanguage = session()->get('language') ?? config('app.locale');
        $languageId = PublicEnum::Language[$nowLanguage]['id'];
        $languageList = json_decode($list, true);
        foreach ($languageList ?? [] as $language) {
            if ($language['id'] == $languageId) {
                $result = $language['txt'];
            }
        }
        return $result;
    }
}
