using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptBuilderUtil.GUI;

namespace ScriptBuilderUtil.Models {

    public class SavedPathModel : Notified {

        bool _choosen;
        public bool Choosen {
            get => _choosen;
            set {
                _choosen = value;
                OnPropertyChanged("Choosen");
            }
        }
        string _path;
        public string Path {
            get => _path;
            set {
                _path = value;
                OnPropertyChanged("Path");
            }
        }

        public SavedPathModel(string path) {
            Path = path;
        }

        /// <summary>
        /// This constructor required for deserialization        
        /// </summary>
        public SavedPathModel() { }

    }
}
