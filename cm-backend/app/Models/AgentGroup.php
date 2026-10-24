<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class AgentGroup extends Model
{
    protected $fillable = [
        'type',
        'organization_id',
        'name',
        'memo',
        'is_default'
    ];

    public function organization()
    {
        return $this->belongsTo(Organization::class);
    }
}
