<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'sms_gateway_group_details';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('agent_group_id')->default(0)->comment('agent_group.id');
                $table->string('setting', 200)->collation('utf8mb4_unicode_ci')->default('{}')->comment('簡訊商資訊, json格式; {"token_expired":"token有效時間", "max_send":"有效時間最大發送次數"}');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('agent_group_id', 'idx_' . $this->table_name . '_' . 'agent_group_id');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'agent_group_id' => 3, 'setting' => '{"token_expired":6,"max_send":3}'],
                ['id' => 2, 'agent_group_id' => 13, 'setting' => '{"token_expired":6,"max_send":3}'],
            ]);
        }
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists($this->table_name);
    }
};
