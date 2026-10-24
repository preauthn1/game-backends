@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.member.profitAnalysis')}}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.general.search')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>UID：</label>
                        <input type="text" class="form-control" wire:model="uid"
                               oninput="value=value.replace(/\D/g,'')">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.username')}}：</label>
                        <input type="text" class="form-control" wire:model="username">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.orderBy')}}：</label>
                        <select class="form-select form-control" wire:model="orderBy">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['orderByList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.sort')}}：</label>
                        <select class="form-select form-control" wire:model="sort">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['sortList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.createdAt')}}：</label>
                        <div class="time-range">
                            <input type="text" name="datetimes" class="form-control" wire:model="timeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body" style="position: relative;">
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th>UID</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalDeposit')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalWithdrawal')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalPurchase')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalRebate')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalReward')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalEarning')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalDepreciation')}}</th>
                            <th>{{__('web_message.transactionType.manualAdd')}}</th>
                            <th>{{__('web_message.transactionType.manualSub')}}</th>
                            <th>{{__('web_message.transactionType.manualReward')}}</th>
                            <th>{{__('web_message.transactionType.manualDeduct')}}</th>
                            <th>{{__('web_message.general.experienceCommission')}}</th>
                            <th class="totalProfit">
                                {{__('web_message.profitAnalysis.totalProfit')}}
                                <i class="bi-question-circle"></i>
                                <div class="tip">{{__('web_message.profitAnalysis.getTotalProfit')}}</div>
                            </th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>{{$data->uid}}</td>
                                <td>{{$data->username}}</td>
                                <td>{{number_format($data->deposit_amount??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->withdrawal_amount??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->purchase_amount??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->rebate_amount??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->reward_amount??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->mining_profits??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->depreciation_amount??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->manual_add??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->manual_sub??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->manual_reward??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->manual_deduct??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->experience_commission??0,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->total_profit??0,session()->get('defaultScale'))}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="15">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="15" class="footable-visible">
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
                    <style>
                        .totalProfit i {
                            cursor: pointer;
                        }

                        .totalProfit .tip {
                            color: #444;
                            display: none;
                            background-color: #ecf5ff;
                            border: 2px solid #c4e1ff;
                            border-radius: 10px;
                            width: 600px;
                            line-height: 30px;
                            padding: 0 5px;
                            position: absolute;
                            top: -20px;
                            right: 5px;
                        }

                        .totalProfit:hover .tip {
                            display: block;
                        }

                        .totalProfit .tip:before,
                        .totalProfit .tip:after {
                            border: solid transparent;
                            content: "";
                            width: 0;
                            height: 0;
                            position: absolute;
                        }

                        .totalProfit .tip:after {
                            border-width: 10px 10px 0 10px;
                            border-top-color: #ecf5ff; /*淺色*/
                            top: 30px;
                            right: 51px;
                        }

                        .totalProfit .tip:before {
                            border-width: 13px 13px 0 13px;
                            border-top-color: #c4e1ff; /*深色*/
                            position: absolute;
                            top: 30px;
                            right: 48px;
                        }
                    </style>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('input[name="datetimes"]').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="datetimes"]').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
                $('input[name="datetimes"]').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
            });
        </script>
    </div>
@endsection
