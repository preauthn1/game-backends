<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class IntervalDailyReport extends Model
{
    protected $table = 'interval_daily_reports';

    protected $fillable = [
        'summary_date',
        'agent_id',
        'regi_count',
        'login_count',
        'active_count',
        'deposit_amount',
        'withdrawal_amount',
        'purchase_amount',
        'purchase_count',
        'rent_amount',
        'rent_count',
        'purchase_income',
        'group_income',
        'rent_income',
        'reward_income',
        'rebate_income',
        'memo',
    ];
}
