//
//  MyUIImagePickerController.m
//  Unity-iPhone
//
//  Created by Eric Benacek on 05/01/12.
//  Copyright (c) 2012 Pointcube. All rights reserved.
//

#import "MyUIImagePickerController.h"

@implementation MyUIImagePickerController

- (void)takePicture{
    
    NSString * string= @"ENTER takePicture2";
    NSLog(@"process Name: %@ Process ID: %d",string);
    
    UnitySendMessage( "EtceteraManager", "imagePickerConfirmShot", "" );
    
    [super takePicture];
}

- (void)imagePickerController:(UIImagePickerController *)picker 
        didFinishPickingImage:(UIImage *)image
                  editingInfo:(NSDictionary *)editingInfo
{
    
    NSString * string= @"ENTER imagePickerController2";
    NSLog(@"process Name: %@ Process ID: %d",string);
}


// update the UI after an image has been chosen or picture taken
//
- (void)cancel
{
    NSString * string= @"ENTER cancel2";
    NSLog(@"process Name: %@ Process ID: %d",string);
    [self.delegate didFinishWithCamera];  // tell our delegate we are done with the camera
    
	UnitySendMessage( "EtceteraManager", "imagePickerDidCancel", "" );
}

@end
