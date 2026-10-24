<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_miner_rebates';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->dateTime('summary_time')->comment('彙整時間');
                $table->dateTime('data_time')->comment('資料時間');
                $table->tinyInteger('type')->default(0)->comment('返水類型; 0:rent, 1:purchase, 2:group');
                $table->integer('member_id')->comment('會員系統編號(members.id)');
                $table->integer('miner_id')->default(0)->comment('礦機系統編號(miners.id)');
                $table->integer('member_miner_id')->default(0)->comment('會員礦機系統編號(member_miners.id)');
                $table->decimal('amount', 30, 10)->default(0.0000000000)->comment('返水金額');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('返水相關資訊');
                $table->string('memo', 500)->collation('utf8mb4_unicode_ci')->nullable()->comment('備註');
                $table->tinyInteger('status')->default(0);
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique(['member_id', 'miner_id', 'summary_time', 'data_time', 'type'], 'uni_member_miner_rebates_1');
                //index
                $table->index('summary_time', 'idx_' . $this->table_name . '_' . 'summary_time');
                $table->index('data_time', 'idx_' . $this->table_name . '_' . 'data_time');
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
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
