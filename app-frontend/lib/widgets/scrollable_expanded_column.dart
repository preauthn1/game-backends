import 'package:flutter/material.dart';

class ScrollableExpandedColumn extends StatelessWidget {
  final List<Widget> children;
  const ScrollableExpandedColumn({super.key, required this.children});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        return SingleChildScrollView(
          child: ConstrainedBox(
            constraints: BoxConstraints(
              minHeight: constraints.maxHeight,
            ),
            child: SafeArea(
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: children,
                ),
              ),
            ),
          ),
        );
      },
    );
  }
}
