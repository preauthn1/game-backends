<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class AgentSettingDefine extends Model
{
    protected $fillable = [
        'define_id',
        'parent_id',
        'group_type',
        'organization_id',
        'is_visible',
        'is_required',
        'language_text',
        'default_value',
        'data_type',
        'sort',
        'memo',
    ];
}
