@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.member.memberList')}}</h1>
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
                        <div class="checkbox">
                            <input id="fuzzy" class="magic-checkbox" type="checkbox" wire:model="fuzzy">
                            <label for="fuzzy">{{__('web_message.search.fuzzy')}}</label>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>UID：</label>
                        <input type="text" class="form-control" wire:model="uid"
                               oninput="value=value.replace(/\D/g,'')">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.agentName')}}：</label>
                        <input type="text" class="form-control" wire:model="agentName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.realName')}}：</label>
                        <input type="text" class="form-control" wire:model="realName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.mobileNo')}}：</label>
                        <select class="form-select form-control" wire:model="countryId">
                            <option value="0">{{__('web_message.general.select')}}</option>
                            @forelse ((session()->get('pageCacheData')['countryCodeList']??[]) as $data)
                                <option value="{{$data->country_id}}">{{$data->country_code}}</option>
                            @empty
                            @endforelse
                        </select>
                        <input type="text" class="form-control" wire:model="mobileNo"
                               oninput="value=value.replace(/\D/g,'')">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.referralCode')}}：</label>
                        <input type="text" class="form-control" wire:model="referralCode">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.registerIp')}}：</label>
                        <input type="text" class="form-control" wire:model="registerIp">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.lastLoginIp')}}：</label>
                        <input type="text" class="form-control" wire:model="lastLoginIp">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.memberType.title')}}：</label>
                        <select class="form-select form-control" wire:model="memberType">
                            <option value="0" selected>{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('web_message.general.memberType.type1')}}</option>
                            <option value="2">{{__('web_message.general.memberType.type2')}}</option>
                            <option value="3">{{__('web_message.general.memberType.type3')}}</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.bankcard')}}：</label>
                        <input type="text" class="form-control" wire:model="accountNo">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.cryptoAddress')}}：</label>
                        <input type="text" class="form-control" wire:model="cryptoAddr">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.registerTime')}}：</label>
                        <div class="time-range">
                            <input type="text" name="datetimes" class="form-control" wire:model="registerRange" autocomplete="off"
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
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="memberStatus">
                            <option value="-1" selected>{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('param.member.status.1')}}</option>
                            <option value="2">{{__('param.member.status.2')}}</option>
                            <option value="9">{{__('param.member.status.9')}}</option>
                        </select>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addMember">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                            </button>
                        </div>
                        <div class="col-sm-6 table-toolbar-right">
                            <button class="btn btn-mint btn-export" wire:click="doExport">
                                <i class="bi-download"></i> {{__('web_message.general.export')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>UID</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.memberType.title')}}</th>
                            <th>{{__('web_message.general.agentName')}}</th>
                            <th>{{__('web_message.general.realName')}}</th>
                            <th>{{__('web_message.general.countryCode')}}</th>
                            <th>{{__('web_message.general.mobileNo')}}</th>
                            <th>{{__('web_message.general.referralCode')}}</th>
                            <th>{{__('web_message.general.balance')}}</th>
                            <th>{{__('web_message.general.registerIp')}}</th>
                            <th>{{__('web_message.general.registerTime')}}</th>
                            <th>{{__('web_message.general.lastLoginIp')}}</th>
                            <th>{{__('web_message.general.lastLoginTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $member)
                            <tr>
                                <td>
                                    <button class="btn btn-warning" wire:click="showModify({{$member->id}})"
                                            id="showModify">
                                        <i class="bi-pencil-square"></i>
                                    </button>
                                    <button class="btn btn-danger" wire:click="doDetail({{$member->id}})">
                                        <i class="bi-person-lines-fill"></i>
                                    </button>
                                </td>
                                <td {{($member->status!=1) ? "class=not-effect" : ''}}>{{$member->uid}}</td>
                                @if(!empty($member->memo))
                                    <td class='has-memo' tooltip="{{__('web_message.general.remark').'： '.$member->memo}}">
                                        {{$member->username}}
                                    </td>
                                @else
                                    <td>{{$member->username}}</td>
                                @endif
                                <td>{{$member->typeTxt}}</td>
                                <td>{{$member->parent_username}}</td>
                                <td>{{$member->real_name}}</td>
                                <td>{{$member->country_code??''}}</td>
                                <td {{($member->mobile_count>1) ? "class=same-mobile" : ''}}>{{$member->mobile}}</td>
                                <td>{{$member->invite_code}}</td>
                                <td tooltip="{{floatval($member->balance)}}">{{number_format($member->balance,session()->get('defaultScale'))}}</td>
                                <td>{{$member->register_ip}}</td>
                                <td>{{$member->register_time}}</td>
                                <td {{($member->lastLoginIp_count>1) ? "class=same-ip" : ''}}>{{$member->last_login_ip}}</td>
                                <td>{{$member->last_login_time}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="14">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="14" class="footable-visible">
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
        <style>
            td.same-ip,
            td.same-mobile ,
            td.has-memo {
                color: red !important;
            }

            td.not-effect {
                color: blue !important;
            }

        </style>
        <!-- addMember -->
        <div class="modal fade" id="addMember" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.member.addMember')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addMember">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.memberType.title')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <div class="radio">
                                            <input id="member-type-radio-2" class="magic-radio" type="radio"
                                                   value="2" wire:model="addMemberType">
                                            <label
                                                for="member-type-radio-2">{{__('web_message.general.memberType.type2')}}</label>
                                        </div>
                                        <div class="radio">
                                            <input id="member-type-radio-3" class="magic-radio" type="radio"
                                                   value="3" wire:model="addMemberType">
                                            <label
                                                for="member-type-radio-3">{{__('web_message.general.memberType.type3')}}</label>
                                        </div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addMemberUsername">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.password')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addMemberPassword">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.fundPassword')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addMemberFundPassword">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.agentName')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addMemberParent">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.realName')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="addMemberRealName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.countryCode')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="addMemberCountryId">
                                            <option value="0">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['countryCodeList']??[]) as $data)
                                                <option value="{{$data->country_id}}">{{$data->country_code}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.mobileNo')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addMemberMobileNo"
                                               oninput="value=value.replace(/\D/g,'')">
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
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
        <!-- modifyMember -->
        <div class="modal fade in" id="modifyMember" role="dialog"
             style="display: {{'modifyMember' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifyClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.member.modifyMember')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModify">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyMid">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.username')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="modifyUsername" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.realName')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyRealName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.countryCode')}}</label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="modifyCountryId">
                                            <option value="0">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['countryCodeList']??[]) as $data)
                                                <option value="{{$data->country_id}}">{{$data->country_code}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.mobileNo')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="modifyMobileNo"
                                               oninput="value=value.replace(/\D/g,'')">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.email')}}</label>
                                    <div class="col-sm-8">
                                        <input type="text" class="form-control" wire:model="modifyEmail">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.reset')}}{{__('web_message.general.password')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="modifyPassword">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.reset')}}{{__('web_message.general.fundPassword')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="modifyFundPassword">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.reset')}}{{__('web_message.general.cryptoAddress')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control" wire:model="modifyCryptoId">
                                            <option
                                                value="0">{{__('web_message.general.select')}} {{__('web_message.general.currency')}}</option>
                                            @forelse ((session()->get('pageCacheData')['cryptoIdList']??[]) as $data)
                                                <option value="{{$data->crypto_id}}">{{$data->crypto_name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label"></label>
                                    <div class="col-sm-8">
                                        <input type="text" class="form-control" wire:model="modifyCryptoAddress"
                                               placeholder="{{__('web_message.general.input')}} {{__('web_message.general.cryptoAddress')}}">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.reset')}}{{__('web_message.general.bankcard')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control" wire:model="modifyBankId">
                                            <option
                                                value="0">{{__('web_message.general.select')}} {{__('web_message.general.bank')}}</option>
                                            @forelse ((session()->get('pageCacheData')['bankIdList']??[]) as $data)
                                                <option value="{{$data->bank_id}}">{{$data->bank_name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label"></label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyBankAccount"
                                               placeholder="{{__('web_message.general.input')}} {{__('web_message.general.bankAccountName')}}">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label"></label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyBankcard"
                                               placeholder="{{__('web_message.general.input')}} {{__('web_message.general.bankcard')}}">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.remark')}}</label>
                                    <div class="col-sm-8">
                                        <textarea rows="3" class="form-control"
                                                  wire:model="modifyMemo"></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.googleAuthCode')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="modifyGoogleAuthCode">
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.modify')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
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
                Livewire.dispatch('registerRange', {range: range});
            });
            $('input[name="datetimes"]').on('cancel.daterangepicker', function (ev, picker) {
                let range = '';
                $(this).val(range);
                Livewire.dispatch('registerRange', {range: range});
            });

            $('#modifyClose').click(function () {
                $('#modifyMember').removeClass('in').css('display', 'none');
                $('.modal-backdrop').css('display', 'none');
            });
        });
    </script>
@endsection
