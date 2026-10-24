<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_miner_rewards';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('type')->comment('獎勵類型');
                $table->dateTime('summary_time')->comment('彙整時間');
                $table->dateTime('data_time')->comment('資料時間');
                $table->integer('member_id')->comment('會員系統編號(members.id)');
                $table->integer('miner_reward_id')->default(0);
                $table->decimal('amount', 30, 10)->default(0.0000000000)->comment('獎勵金額');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('獎勵相關資訊');
                $table->string('memo', 500)->collation('utf8mb4_unicode_ci')->nullable()->comment('備註');
                $table->tinyInteger('status')->default(0);
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('summary_time', 'idx_' . $this->table_name . '_' . 'summary_time');
                $table->index('data_time', 'idx_' . $this->table_name . '_' . 'data_time');
                $table->index('type', 'idx_' . $this->table_name . '_' . 'type');
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('miner_reward_id', 'idx_' . $this->table_name . '_' . 'miner_reward_id');
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
