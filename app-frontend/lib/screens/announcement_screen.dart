import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/announcement.dart';
import '../providers/promotion_provider.dart';
import '../themes/app_colors.dart';
import '../widgets/base_card.dart';
import '../widgets/base_tab_bar.dart';
import '../widgets/linear_gradient_background.dart';

class AnnouncementScreen extends StatefulWidget {
  const AnnouncementScreen({super.key});

  @override
  State<AnnouncementScreen> createState() => _AnnouncementScreenState();
}

class _AnnouncementScreenState extends State<AnnouncementScreen> {
  late Future<void> _future;

  Future<void> _fetchData() async {
    await context.read<PromotionProvider>().fetchAnnouncement(0);
  }

  Widget _buildList(List<Announcement> items) {
    return SingleChildScrollView(
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          children: [
            for (var i = 0; i < items.length; i++) ...[
              BaseCard(
                width: double.infinity,
                padding:
                    const EdgeInsets.symmetric(vertical: 20, horizontal: 16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      items[i].startTime ?? '-',
                      style: const TextStyle(fontSize: 12),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      items[i].announcementTitle ?? '-',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: AppColors.primaryColor,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      items[i].announcementContent ?? '',
                      style: const TextStyle(
                        color: Color(0xFF8c8b8d),
                      ),
                    ),
                  ],
                ),
              ),
              if (i != items.length - 1) const SizedBox(height: 10),
            ]
          ],
        ),
      ),
    );
  }

  @override
  void initState() {
    _future = _fetchData();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: FutureBuilder(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return Scaffold(
              appBar: AppBar(title: const Text('Announcement')),
              body: const Center(child: CircularProgressIndicator()),
            );
          }

          if (snapshot.hasError) {
            return Scaffold(
              appBar: AppBar(title: const Text('Announcement')),
              body: const Center(
                child: Text('An error occurred. Please try again later.'),
              ),
            );
          }

          final allAnnouncements =
              context.watch<PromotionProvider>().announcements;
          final systemAnnouncements = allAnnouncements
              .where((announcement) => announcement.announcementType == '1')
              .toList();
          final promotionAnnouncements = allAnnouncements
              .where((announcement) => announcement.announcementType == '2')
              .toList();
          final platformAnnouncements = allAnnouncements
              .where((announcement) => announcement.announcementType == '3')
              .toList();

          return DefaultTabController(
            length: 3,
            child: Scaffold(
              appBar: AppBar(
                title: const Text('Announcement'),
                bottom: const BaseTabBar(
                  tabs: [
                    Tab(text: 'System'),
                    Tab(text: 'Promotion'),
                    Tab(text: 'Platform'),
                  ],
                ),
              ),
              body: TabBarView(
                children: [
                  _buildList(systemAnnouncements),
                  _buildList(promotionAnnouncements),
                  _buildList(platformAnnouncements),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
