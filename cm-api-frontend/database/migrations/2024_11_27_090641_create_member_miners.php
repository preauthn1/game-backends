<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_miners';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->tinyInteger('type')->default(0)->comment('類型: 0:rental, 1:purchase, 2:group');
                $table->integer('member_id')->comment('會員系統編號(members.id)');
                $table->integer('miner_id')->comment('礦機系統編號(miner_id)');
                $table->integer('parent_id')->nullable();
                $table->dateTime('check_time')->nullable()->comment('確認時間 訂單真正成立時間');
                $table->dateTime('start_time')->nullable()->comment('開始時間');
                $table->dateTime('end_time')->nullable()->comment('結束時間');
                $table->dateTime('can_sellback_time')->nullable()->comment('可售回時間');
                $table->dateTime('cancel_time')->nullable()->comment('取消時間');
                $table->tinyInteger('status')->default(0)->comment('0:訂單建立; 1:進行中; 2:已取消/已到期; 3:取消中;');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('備註資訊');
                $table->integer('operator')->default(0)->comment('操作者');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('miner_id', 'idx_' . $this->table_name . '_' . 'miner_detail_id');
                $table->index('start_time', 'idx_' . $this->table_name . '_' . 'start_time');
                $table->index('end_time', 'idx_' . $this->table_name . '_' . 'end_time');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
                $table->index('parent_id', 'idx_' . $this->table_name . '_' . 'parent_id');
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
