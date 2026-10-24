import 'package:flutter/material.dart';

import '../models/announcement.dart';
import '../models/home_information.dart';
import '../models/promotion.dart';
import '../utils/http_service.dart';

class PromotionProvider with ChangeNotifier {
  List<Announcement> _announcements = [];
  HomeInformation? _homeInformation;
  List<Promotion> _promotions = [];

  List<Promotion> get promotions => _promotions;
  HomeInformation? get homeInformation => _homeInformation;
  List<Announcement> get announcements => _announcements;

  void clear() {
    _announcements = [];
    _homeInformation = null;
    _promotions = [];
    notifyListeners();
  }

  Future<void> fetchAnnouncement(int type) async {
    final res = await HttpService.post(
      path: "api/announcement/list",
      data: {
        //公告類別; 0:全部, 1:系統, 2:優惠, 3:平台
        'announcement_type': type,
      },
    );
    _announcements = List<Announcement>.from(
      res['data']['list'].map((item) => Announcement.fromJson(item)),
    );
    notifyListeners();
  }

  Future<void> fetchHomeInfo() async {
    final res = await HttpService.post(path: "api/home/information");
    _homeInformation = HomeInformation.fromJson(res['data']);
    notifyListeners();
  }

  Future<void> fetchPromotions() async {
    final res = await HttpService.post(path: 'api/promotion/list');
    _promotions = List<Promotion>.from(
      res['data']['list'].map((item) => Promotion.fromJson(item)),
    );
    notifyListeners();
  }
}
