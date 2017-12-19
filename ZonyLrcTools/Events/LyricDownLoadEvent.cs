﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus;
using Zony.Lib.Infrastructures.EventBus.Handlers;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using ZonyLrcTools.Common;

namespace ZonyLrcTools.Events
{
    public class LyricDownLoadEventData : EventData
    {
        /// <summary>
        /// 要下载的音乐集合
        /// </summary>
        public IEnumerable<MusicInfoModel> MusicInfos { get; set; }

        public LyricDownLoadEventData(IEnumerable<MusicInfoModel> musicInfos)
        {
            MusicInfos = musicInfos;
        }
    }

    public class LyricDownLoadEvent : IEventHandler<LyricDownLoadEventData>, ITransientDependency
    {
        private readonly IPluginManager m_pluginManager;

        public LyricDownLoadEvent(IPluginManager pluginManager)
        {
            m_pluginManager = pluginManager;
        }

        public async void HandleEvent(LyricDownLoadEventData eventData)
        {
            IPluginDownLoader _downloader = m_pluginManager.GetPlugin<IPluginDownLoader>();

            await Task.Run(() =>
            {
                Parallel.ForEach(eventData.MusicInfos, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, (info, loopState) =>
                    {
                        try
                        {
                            _downloader.DownLoad(info.Song, info.Artist, out byte[] _lyricData);

                            if (_lyricData == null)
                            {
                                GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[info.Index].SubItems[AppConsts.Status_Position].Text = AppConsts.Status_Music_Failed;
                                return;
                            }

                            // 写入歌词
                            var _eventData = new LyricDownLoadCompleteEventData()
                            {
                                LyricData = _lyricData,
                                Info = info
                            };

                            EventBus.Default.Trigger(_eventData);
                        }
                        catch (NotFoundLyricException)
                        {
                            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items[info.Index].SubItems[4].Text = AppConsts.Status_Music_NotFoundLyric;
                        }
                    });
            });
        }
    }
}