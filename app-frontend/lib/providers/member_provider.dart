// ignore: depend_on_referenced_packages
import 'package:collection/collection.dart';
import 'package:flutter/material.dart';

import '../models/agent_center.dart';
import '../models/avatar.dart';
import '../models/invite.dart';
import '../models/member.dart';
import '../models/referral_reward_record.dart';
import '../models/sales_rebate_record.dart';
import '../utils/http_service.dart';

class MemberProvider with ChangeNotifier {
  Member? _member;
  List<Avatar> _avatarList = [];
  Avatar? _currentAvatar;
  Invite? _invite;
  SalesRebateRecord? _salesRebateRecord;
  ReferralRewardRecord? _referralRewardRecord;
  AgentCenter? _agentCenter;

  Member? get member => _member;
  List<Avatar> get avatarList => _avatarList;
  Avatar? get currentAvatar => _currentAvatar;
  Invite? get invite => _invite;
  SalesRebateRecord? get salesRebateRecord => _salesRebateRecord;
  ReferralRewardRecord? get referralRewardRecord => _referralRewardRecord;
  AgentCenter? get agentCenter => _agentCenter;

  void clear() {
    _member = null;
    _avatarList = [];
    _currentAvatar = null;
    _invite = null;
    _salesRebateRecord = null;
    _referralRewardRecord = null;
    _agentCenter = null;
    notifyListeners();
  }

  Future<void> fetchMember() async {
    final res = await HttpService.post(path: 'api/member/getProfile');
    _member = Member.fromJson(res['data']);
    notifyListeners();
  }

  Future<void> setMember(Member member) async {
    _member = member;
    notifyListeners();
  }

  Future<void> fetchAvatarList() async {
    final res = await HttpService.post(path: 'api/member/avatar');
    _avatarList = List<Avatar>.from(
      res['data']['list'].map((item) => Avatar.fromJson(item)),
    );
    _currentAvatar = _avatarList.firstWhereOrNull(
      (element) => element.id == res['data']?['avatar_id']?.toString(),
    );
    notifyListeners();
  }

  Future<void> setAvatar(Avatar avatar) async {
    await HttpService.post(
      path: 'api/member/setAvatar',
      data: {'avatar_id': avatar.id},
    );
    _currentAvatar = avatar;
    _member?.avatarImage = avatar.image;
    notifyListeners();
  }

  Future<void> setRealName(String value) async {
    await HttpService.post(
      path: 'api/member/createName',
      data: {'name': value},
    );
    _member?.realName = value;
    notifyListeners();
  }

  Future<void> setEmail(String value) async {
    await HttpService.post(
      path: 'api/member/createEmail',
      data: {'email': value},
    );
    _member?.email = value;
    notifyListeners();
  }

  Future<void> uploadIDImage({
    required String front,
    required String back,
  }) async {
    await HttpService.post(
      path: 'api/member/idInformation',
      data: {
        "id_front_image": front,
        "id_back_image": back,
      },
    );
    _member?.idInformationStatus = '0';
    notifyListeners();
  }

  Future<void> createBank({
    required String bankId,
    required String accountNo,
    required String accountName,
  }) async {
    await HttpService.post(
      path: 'api/member/createBank',
      data: {
        "bank_id": bankId,
        "account_no": accountNo,
        "account_name": accountName,
      },
    );
  }

  Future<void> createCryptoAddress({
    required String address,
    required String bankId,
  }) async {
    await HttpService.post(
      path: 'api/member/createCryptos',
      data: {
        "crypto_address": address,
        "bank_id": bankId,
      },
    );
  }

  Future<void> fetchInviteData() async {
    final res = await HttpService.post(path: 'api/member/invite');
    _invite = Invite.fromJson(res['data']);
    notifyListeners();
  }

  Future<void> fetchAgentCenter() async {
    final res = await HttpService.post(path: 'api/record/agentCenter');
    _agentCenter = AgentCenter.fromJson(res['data']);
    notifyListeners();
  }

  Future<void> fetchSalesRebateRecord({
    required int page,
    required int perPage,
    required int days,
  }) async {
    final res = await HttpService.post(path: 'api/record/rebate', data: {
      'days': days,
      'page': page,
      'per_page': perPage,
    });
    _salesRebateRecord = SalesRebateRecord.fromJson(res['data']);
    notifyListeners();
  }

  Future<void> fetchReferralRewardRecord({
    required int page,
    required int perPage,
    required int days,
  }) async {
    final res = await HttpService.post(path: 'api/record/reward', data: {
      'days': days,
      'page': page,
      'per_page': perPage,
    });
    _referralRewardRecord = ReferralRewardRecord.fromJson(res['data']);
    notifyListeners();
  }
}
