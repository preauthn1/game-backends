@extends('livewire.template')
@section('content')
<div id="page-title">
    <h1 class="page-header text-overflow">{{__('web_message.menu.system.miningBlockSetting')}}</h1>
</div>
<div id="page-content">
    <!--資料區塊-->
    <div class="panel">
        <div class="panel-body">
            <div class="pad-btm form-inline">
                <div class="row">
                    <div class="col-sm-6 table-toolbar-left">
                        <button class="btn btn-purple" data-toggle="modal" data-target="#addMiningBlock">
                            <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                        </button>
                    </div>
                </div>
            </div>
            <div class="table-responsive">
                <table class="table table-striped txt-center">
                    <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>ID</th>
                            <th>{{__('web_message.general.currency')}}</th>
                            <th>{{__('web_message.miningBlock.block_height')}}</th>
                            <th>{{__('web_message.miningBlock.block_time')}}</th>
                            <th>{{__('web_message.miningBlock.mining_difficulty')}}</th>
                            <th>{{__('web_message.miningBlock.mining_change')}}(%)</th>
                            <th>{{__('web_message.miningBlock.block_reward')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                            <th>{{__('web_message.general.reviewer')}}</th>
                            <th>{{__('web_message.general.lastUpdateTime')}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                        <tr>
                            @if(0==($data->status??1))
                            <td>
                                <button class="btn btn-warning" wire:click="showDataPop({{$data->id}},'modify')">
                                    <i class="bi-pencil-square"></i>
                                </button>
                                <button class="btn btn-warning" wire:click="showDataPop({{$data->id}},'detail')">
                                    <i class="bi-list-task"></i>
                                </button>
                                <button class="btn btn-danger" wire:click="doDelete({{$data->id}},'detail')">
                                    <i class="bi-trash3"></i>
                                </button>
                            </td>
                            @else
                            <td></td>
                            @endif
                            <td>{{$data->statusTxt}}</td>
                            <td>{{$data->id}}</td>
                            <td>{{$data->currency_name}}</td>
                            <td>{{$data->block_height}}</td>
                            <td>{{$data->block_time}}</td>
                            <td>{{$data->mining_difficulty}}</td>
                            <td>{{$data->mining_change}}</td>
                            <td>{{$data->block_reward}}</td>
                            <td>{{$data->created_at}}</td>
                            <td>{{$data->reviewer}}</td>
                            <td>{{$data->updated_at}}</td>
                        </tr>
                        @empty
                        <tr>
                            <td colspan="12">{{__('web_message.general.noData')}}</td>
                        </tr>
                        @endforelse
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="12" class="footable-visible">
                                <div class="text-right" style="position: relative;">
                                    <div class="total-entries">
                                        {{(session()->get('pageCacheData')['totalRow']??0)}} {{__('web_message.general.entries')}}
                                    </div>
                                    <x-pagination></x-pagination>
                                </div>
                            </td>
                        </tr>
                    </tfoot>
                </table>
            </div>
        </div>
    </div>
    <!-- addMiningBlock -->
    <div class="modal fade" id="addMiningBlock" role="dialog">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">{{__('web_message.menu.system.addMiningBlock')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="addMiningBlock">
                        <div class="panel-body">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.currency')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="addMiningBlockCurrencyId">
                                        <option value="0">{{__('web_message.general.select')}}</option>
                                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                                        <option value="{{$data->currency_id}}">{{$data->currency_name}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.miningBlock.block_height')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="addMiningBlockHeight"
                                        oninput="value=value.replace(/\D/g,'')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.miningBlock.block_time')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" name="dates" autocomplete="off" id="addMiningBlockTime">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.miningBlock.mining_difficulty')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="addMiningBlockDifficulty"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.miningBlock.mining_change')}}(%)
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="addMiningBlockChange"
                                        oninput="value=value.replace(/[^-\d.]/g,'').replace(/(?!^)-/g,'').replace(/^(-?\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.miningBlock.block_reward')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="addMiningBlockReward"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                        </div>
                        <div class="panel-footer">
                            <div class="row" style="position: relative;">
                                <div class="col-sm-offset-10">
                                    @if(!empty(session()->get('pageCacheData')['miningBlockUrl']??''))
                                    <a class="btn btn-default" target="_blank" style="position: absolute;left: 5%;"
                                        href="{{session()->get('pageCacheData')['miningBlockUrl']??''}}">
                                        {{__('web_message.general.references')}}
                                    </a>
                                    @endif
                                    <button class="btn btn-mint"
                                        type="submit">{{__('web_message.general.add')}}</button>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
    <!-- modifyMiningBlock -->
    <div class="modal fade in" id="modifyMiningBlock" role="dialog"
        style="display: {{'modifyMiningBlock' == session('showPopId') ? 'block':'none'}};">
        <div class="modal-backdrop fade in" style="display: block;"></div>
        <div class="modal-dialog" style="z-index: 1050;">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" id="modifyClose">&times;</button>
                    <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.menu.system.modifyMiningBlock'):__('web_message.menu.system.miningBlockDetail')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="doModify">
                        <div class="panel-body">
                            <input type="hidden" wire:model="popMiningBlockId">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.general.currency')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="modifyMiningBlockCurrencyId" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                        <option value="0">{{__('web_message.general.select')}}</option>
                                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                                        <option value="{{$data->currency_id}}">{{$data->currency_name}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.miningBlock.block_height')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyMiningBlockHeight" oninput="value=value.replace(/\D/g,'')" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.miningBlock.block_time')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" name="dates" autocomplete="off" id="modifyMiningBlockTime" wire:model="modifyMiningBlockTime" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.miningBlock.mining_difficulty')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyMiningBlockDifficulty" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.miningBlock.mining_change')}}(%)
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyMiningBlockChange" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}
                                        oninput="value=value.replace(/[^-\d.]/g,'').replace(/(?!^)-/g,'').replace(/^(-?\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.miningBlock.block_reward')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyMiningBlockReward" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.general.status')}}</label>
                                <div class="col-sm-6">
                                    <select class="form-select form-control"
                                        wire:model="modifyMiningBlockStatus" disabled>
                                        <option value="-1">{{__('web_message.general.select')}}</option>
                                        @forelse((session()->get('pageCacheData')['statusList']??[]) as $data)
                                        <option value="{{$data->id}}">{{$data->name}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                        </div>
                        @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-10">
                                    <button class="btn btn-mint"
                                        type="submit">{{__('web_message.general.modify')}}</button>
                                </div>
                            </div>
                        </div>
                        @endif
                        @if('detail'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-9">
                                    <div class="btn btn-danger" wire:confirm="{{__('web_message.miningBlock.approved_tip')}}"
                                        wire:click="doApprove()">{{__('web_message.general.approved')}}</div>
                                </div>
                            </div>
                        </div>
                        @endif
                    </form>
                </div>
            </div>
        </div>
    </div>
    <!--頁面特殊JS-->
    <script>
        $(function() {
            $('#modifyClose').click(function() {
                $('#modifyMiningBlock').removeClass('in').css('display', 'none');
                $('.modal-backdrop').css('display', 'none');
            });
            $('input[name="dates"]').daterangepicker({
                singleDatePicker: true,
                timePicker: true,
                timePicker24Hour: true,
                timePickerSeconds: true,
                autoUpdateInput: false,
                locale: {
                    format: 'YYYY-MM-DD HH:mm:ss'
                },
                drops: 'down',
            });
            $('#addMiningBlockTime').on('apply.daterangepicker', function(ev, picker) {
                let date = picker.startDate.format('YYYY-MM-DD HH:mm:ss');
                $(this).val(date);
                Livewire.dispatch('addMiningBlockTime', {
                    date: date
                });
            });
            $('#addMiningBlockTime').on('cancel.daterangepicker', function(ev, picker) {
                let date = '';
                $(this).val(date);
                Livewire.dispatch('addMiningBlockTime', {
                    date: date
                });
            });
            $('#addMiningBlockTime').blur(function() {
                Livewire.dispatch('addMiningBlockTime', {
                    date: $(this).val()
                });
            });
            $('#modifyMiningBlockTime').on('apply.daterangepicker', function(ev, picker) {
                let date = picker.startDate.format('YYYY-MM-DD HH:mm:ss');
                $(this).val(date);
                Livewire.dispatch('modifyMiningBlockTime', {
                    date: date
                });
            });
            $('#modifyMiningBlockTime').on('cancel.daterangepicker', function(ev, picker) {
                let date = '';
                $(this).val(date);
                Livewire.dispatch('modifyMiningBlockTime', {
                    date: date
                });
            });
            $('#modifyMiningBlockTime').blur(function() {
                Livewire.dispatch('modifyMiningBlockTime', {
                    date: $(this).val()
                });
            });
        });
    </script>
</div>
@endsection