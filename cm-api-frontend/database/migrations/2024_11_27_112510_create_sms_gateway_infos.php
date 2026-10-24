<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'sms_gateway_infos';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('agent_group_id')->default(0)->comment('簡訊群組ID(agent_groups.id)');
                $table->unsignedInteger('sms_gateway_id')->default(0)->comment('簡訊商ID');
                $table->string('info', 1000)->collation('utf8mb4_unicode_ci')->default('{}')->comment('簡訊商商戶資訊, 內容使用json儲存');
                $table->integer('sort')->default(0)->comment('簡訊商排序');
                $table->integer('status')->default(0)->comment('狀態; 0:停用, 1:啟用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('agent_group_id', 'idx_' . $this->table_name . '_' . 'agent_group_id');
                $table->index('sms_gateway_id', 'idx_' . $this->table_name . '_' . 'sms_gateway_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'agent_group_id' => 3, 'sms_gateway_id' => 1, 'info' => '{}', 'status' => 0],
                ['id' => 2, 'agent_group_id' => 13, 'sms_gateway_id' => 1, 'info' => '{}', 'status' => 0],
                ['id' => 3, 'agent_group_id' => 3, 'sms_gateway_id' => 2, 'info' => '{"SpCode":"100539","LoginName":"mb8888","Password":"qM1dFQsKC","insert_characters":""}', 'status' => 1],
                ['id' => 4, 'agent_group_id' => 13, 'sms_gateway_id' => 2, 'info' => '{"SpCode":"100539","LoginName":"mb8888","Password":"qM1dFQsKC","insert_characters":""}', 'status' => 1],
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
