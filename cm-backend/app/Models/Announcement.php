<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Carbon\Carbon;

class Announcement extends Model
{
    protected $fillable = [
        'type',
        'title',
        'content',
        'image',
        'redirect_url',
        'is_top',
        'start_time',
        'end_time',
        'status'
    ];

    public function getTypeTxtAttribute()
    {
        $types = [
            1 => __('param.announcement.type.1'),
            2 => __('param.announcement.type.2'),
            3 => __('param.announcement.type.3')
        ];

        return $types[$this->type] ?? __('param.other');
    }

    // 定義 statusTxt 虛擬屬性
    public function getStatusTxtAttribute()
    {
        return $this->status == 1 ? __('param.enable') : __('param.disable');
    }

    public function getStartTimeAttribute($value)
    {
        return Carbon::parse($value, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
    }

    public function getEndTimeAttribute($value)
    {
        return Carbon::parse($value, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
    }
}
