   M S B A M L   `   `   `  ����D  @Project K, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:Project_K.ViewProject_K.View  5   	   9!clr-namespace:Project_K.ViewModelProject_K.ViewModel  5   	   I)clr-namespace:Project_K.ViewModel.HelpersProject_K.ViewModel.Helpers  5   	       Project_K.View.MeterType   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:Project_K.View     5   	   /l!clr-namespace:Project_K.ViewModel     5   	   8l1)clr-namespace:Project_K.ViewModel.Helpers     5   	     � Title$  	MeterType��5
   	   $	��450��6   $	��800��6(   $	��600��64   $
��1000��6D    � 
ResizeMode$ CanResizeWithGrip=�6T   
������� � DataContext 5      !   Project_K.ViewModel.MeterVM  5   
    � 	Resources 5      9   3Project_K.ViewModel.Helpers.StringToDoubleConverter   StringToDoubleConverter��%   &	          5   
   6&   5      .��5      �  � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   � $��0��6   $
��Auto��6    � RowDefinitions 5      �� 5      $��50 �6    �� 5      $��* �6    .+�5      �� $��0��6   $��Meters Available��6%   $	��Top=�6=   $��20(�6U   $��
50,20,30,0q�6c   $��Center=�6w   �� 5      $��1��6   $
��Auto=�6(   $��Disabled=�6K   $��
50,0,30,80q�6t   .��5      �� $��1��6   $	��Top=�6O   $��
50,0,30,80q�6g   ��6,   �� *
MeterNames+��5       �� 5!      .R�5"      �� $��20(�69   ��6(   �� 69   5#      5%      5&      5'      � 5)   
   $��1��6    5*      �� 5+      $
��Auto �6"   �� 5,      $
��Auto �6"   .+�5/      � $��0��6   $��Center=�6    $��Center=�6=   $��
0,30,80,10q�6X    50      �� 51      $��* �6'   �� 52      $��25 �6'   �� 53      $��* �6'   .+�56      � $��0��6    57      �� 58      $	��50* �6(   �� 59      $	��50* �6(   �� 5:      $	��50* �6(   �� 5;      $	��50* �6(   �� 5<      $	��50* �6(   .+�5@      �� $��0��6    $��Meter ID/Name��6-   $��Right=�6B   $��Center=�6^   $��Cambria)�6y   $��20(�6�   �� 5A      $��1��6    $
��Make��6-   $��Right=�69   $��Center=�6U   $��Cambria)�6p   $��20(�6�   �� 5B      $��2��6    $��Meter Type ��6-   $��Right=�6@   $��Center=�6\   $��Cambria)�6w   $��20(�6�   �� 5C      $��3��6    $��
IP Address��6-   $��Right=�6?   $��Center=�6[   $��Cambria)�6v   $��20(�6�   �� 5D      $��4��6    $��Meter Description ��6-   $��Right=�6G   $��Center=�6c   $��Cambria)�6   $��20(�6�   5E      � 5G      $��2��6    5H      �� 5I      $	��50* �6(   �� 5J      $	��50* �6(   �� 5K      $	��50* �6(   �� 5L      $	��50* �6(   �� 5M      $	��50* �6(   .+�5Q      �� -   5         ��Name$ 	meterName��5Q   +   $��0��6   $
��Left=�6>   $��Center=�6Y   $��20(�6t   $��40��6�   
�������$��5q�6�   $	��300��6�   ��6�   �� *MeterDetails.MeterName+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=��� 5R      -   5        $
 make��5R   +   $��1��6   $
��Left=�69   $��Center=�6T   $��20(�6o   $��40��6}   
�������$��5q�6�   $	��300��6�   ��6�   �� *MeterDetails.Make+$ TwoWay=�$ PropertyChanged=��� 5S      -   5        $ 	meterType��5S   +   $��2��6   $
��Left=�6>   $��Center=�6Y   $��20(�6t   $��40��6�   
�������$��5q�6�   $	��300��6�   ��6�   �� *MeterDetails.MeterType+$ TwoWay=�$ PropertyChanged=��� 5T      -   5        $ 	iPAddress��5T   +   $��3��6   $
��Left=�6>   $��Center=�6Y   $��20(�6t   $��40��6�   
�������$��5q�6�   $	��300��6�   ��6�   �� *MeterDetails.IPAddress+$ TwoWay=�$ PropertyChanged=��� 5U      -   5        $ meterDescription��5U   +   $��4��6   $
��Left=�6E   $��Center=�6`   $��20(�6{   
�������$��5q�6�   $	��300��6�   $��50��6&  ��6�   �� *MeterDetails.MeterDescription+$ TwoWay=�$ PropertyChanged=�6&  5V      5W      �� 5Y      $��1��6   $��Download��6"   $��-200,0,100,0q�65   $	��Top=�6L   $��30��6d   $��80��6p   ��6{   �� *DownloadCommand+�� 5Z      $��1��6   $��Refresh��6"   $��	0,0,100,0q�64   $	��Top=�6H   $��30��6`   $��80��6l   ��6w   �� *RefreshCommand+�� 5[      $��1��6   $	��Add��6"   $��200,0,100,0q�60   $	��Top=�6E   $��30��6]   $��80��6i   ��6t   �� *SaveMeterDetailsCommand+�� 5\      $��1��6   $��Remove��6"   $��	300,0,0,0q�63   $	��Top=�6F   $��30��6^   $��80��6j   ��6u   �� *RemoveMeterDetailsCommand+5]      5^      5`      