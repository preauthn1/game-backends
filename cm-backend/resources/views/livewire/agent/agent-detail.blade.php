@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.agent.agentDetail')}}</h1>
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
                        <label>{{__('web_message.general.username')}}：</label>
                        <input type="text" class="form-control" wire:model="username">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.childLevel')}}：</label>
                        <select class="form-select form-control" wire:model="childLevel">
                            <option value="0">{{__('web_message.general.select')}}</option>
                            @for ($i = 1; $i < ($maxLevel??0); $i++)
                                <option value="{{$i}}">{{__('param.member_child.level').$i}}</option>
                            @endfor
                        </select>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--彙總區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th>{{__('web_message.profitAnalysis.totalDeposit')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalDepositCount')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalWithdrawal')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalWithdrawalCount')}}</th>
                            <th>{{__('web_message.profitAnalysis.totalBalance')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        <tr>
                            <td>{{number_format(session()->get('pageCacheData')['summaryData']->deposit??0,session()->get('defaultScale'))}}</td>
                            <td>{{session()->get('pageCacheData')['summaryData']->deposit_member_count??0}}</td>
                            <td>{{number_format(session()->get('pageCacheData')['summaryData']->withdrawal??0,session()->get('defaultScale'))}}</td>
                            <td>{{session()->get('pageCacheData')['summaryData']->withdrawal_member_count??0}}</td>
                            <td>{{number_format(session()->get('pageCacheData')['summaryData']->balance??0,session()->get('defaultScale'))}}</td>
                        </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="table-responsive">
                    <table class="table txt-center">
                        <thead>
                        <tr>
                            <th>{{__('web_message.general.childLevel')}}</th>
                            <th>UID</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.agentName')}}</th>
                            <th>{{__('web_message.general.balance')}}</th>
                            <th>{{__('web_message.general.minerResidual')}}</th>
                            <th>{{__('web_message.general.registerTime')}}</th>
                            <th>{{__('web_message.general.minerName')}}</th>
                            <th>{{__('web_message.general.purchaseTotalCount')}}</th>
                            <th>{{__('web_message.general.purchaseTotalPrice')}}</th>
                            <th>{{__('web_message.general.sellBackTotalCount')}}</th>
                            <th>{{__('web_message.general.sellBackTotalPrice')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            @if($data->minerCount > 0)
                                @foreach($data->miner as $index => $miner)
                                    <tr @if($seq % 2 == 0) class="odd-row" @endif>
                                        @if($index === 0)
                                            <td rowspan="{{ $data->minerCount }}">{{ $data->level }}</td>
                                            <td rowspan="{{ $data->minerCount }}">{{ $data->uid }}</td>
                                            <td rowspan="{{ $data->minerCount }}">{{ $data->username }}</td>
                                            <td rowspan="{{ $data->minerCount }}">{{ $data->parent_username }}</td>
                                            <td rowspan="{{ $data->minerCount }}">{{ number_format($data->balance ?? 0, session()->get('defaultScale')) }}</td>
                                            <td rowspan="{{ $data->minerCount }}">{{ number_format($data->residual_value ?? 0, session()->get('defaultScale')) }}</td>
                                            <td rowspan="{{ $data->minerCount }}">{{ $data->created_at }}</td>
                                        @endif
                                        <td>{{ $miner->name }}</td>
                                        <td>{{ number_format($miner->total_count ?? 0) }}</td>
                                        <td>{{ number_format($miner->total_price ?? 0, session()->get('defaultScale')) }}</td>
                                        <td>{{ number_format($miner->total_sellback_count ?? 0) }}</td>
                                        <td>{{ number_format($miner->total_sellback_price ?? 0, session()->get('defaultScale')) }}</td>
                                    </tr>
                                @endforeach
                            @else
                                <tr @if($seq % 2 == 0) class="odd-row" @endif>
                                    <td>{{ $data->level }}</td>
                                    <td>{{ $data->uid }}</td>
                                    <td>{{ $data->username }}</td>
                                    <td>{{ $data->parent_username }}</td>
                                    <td>{{ number_format($data->balance ?? 0, session()->get('defaultScale')) }}</td>
                                    <td>{{ number_format($data->residual_value ?? 0, session()->get('defaultScale')) }}</td>
                                    <td>{{ $data->created_at }}</td>
                                    <td colspan="5">{{ __('web_message.general.noData') }}</td>
                                </tr>
                            @endif
                        @empty
                            <tr class="odd-row">
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
                    <style>
                        tr.odd-row td {
                            background-color: #f2f2f2;
                        }
                    </style>
                </div>
            </div>
        </div>
    </div>
@endsection
