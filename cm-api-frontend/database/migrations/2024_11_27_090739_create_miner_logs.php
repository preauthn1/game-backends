<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'miner_logs';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('agent_id')->default(0)->comment('操作人員系統編號');
                $table->integer('miner_id')->default(0)->comment('礦機系統編號');
                $table->text('before_info')->collation('utf8mb4_unicode_ci')->comment('修改前相關資訊(json)');
                $table->text('after_info')->collation('utf8mb4_unicode_ci')->comment('修改後相關資訊(json)');
                $table->string('remark', 45)->nullable()->comment('備註');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('agent_id', 'idx_' . $this->table_name . '_' . 'agent_id');
                $table->index('miner_id', 'idx_' . $this->table_name . '_' . 'miner_id');
            });
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
